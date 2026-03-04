#!/usr/bin/env bash
# run_p2_sequential.sh - Guitar_Project p2-* cron job 순차 실행기
# openclaw cron run --expect-final 로 한 job이 완료될 때까지 대기 후 다음 실행
# 실패 시 즉시 중단 (상태파일/소스 충돌 방지)
set -Eeuo pipefail

# 타임아웃: job당 최대 10분 (복잡한 AI 작업 대비)
TIMEOUT_MS=600000

# p2-* 순차 실행 순서 (이름 기준 정렬)
# 중복 job 있을 경우 cron 스케줄 버전 우선 (at 2035... 버전 제외)
JOBS=(
  "76bd55d8-584d-466d-9cb2-442451dd4bc2"  # p2-a1a-songdata-schema
  "f972e9e8-ca0b-43f1-81e7-82ac38d5c916"  # p2-a1b-songs-get-endpoint
  "12b9162f-e1ac-4475-9ab6-3c4da6e36533"  # p2-a1c-songs-post-endpoint
  "b109f057-7f27-4470-a7c3-7910d5c4ce42"  # p2-a2a-session-start
  "168d41d9-1c7e-4473-a8b0-e84a0787c8ea"  # p2-a2b-session-loop
  "344d8eda-97f0-4086-b182-b10a7c094a2f"  # p2-a2c-session-state-end
  "b588e22f-2861-4f88-8d3a-5195764f79f3"  # p2-a3a-test-songs
  "4921a035-fdfc-4d8b-94ea-7689bf6dccd9"  # p2-a3b-test-session
  "2a7a7f95-4c57-40e4-8ab0-e489444d51d8"  # p2-b4a-spec-songdata-cs
  "d2c72966-906c-4e8e-920b-b65052092312"  # p2-b4b-spec-songselector-ui
  "450b8667-33d7-4a94-8a5c-23d1f32478a2"  # p2-b5a-spec-practiceconfig
  "ebe40b3a-dd13-4404-8d51-8e02a36ff5d7"  # p2-b5b-spec-practicesession-up
  "db79c560-810b-4c6c-9b7c-f71caa65b087"  # p2-b6-spec-transpose
  "0b6dd595-93c7-4e50-9573-10001f3af823"  # p2-c7a-pitch-smoothing
  "860c5f64-d3f1-4f38-8813-d76dd914a665"  # p2-c8-server-validation
)

TOTAL=${#JOBS[@]}
PASSED=0
FAILED=0
FAILED_JOBS=()

log() { echo "[$(date '+%F %T')] $*"; }

log "=== p2-* 순차 실행 시작 (총 ${TOTAL}개) ==="

for i in "${!JOBS[@]}"; do
  JOB_ID="${JOBS[$i]}"
  NUM=$((i + 1))

  log "▶ [${NUM}/${TOTAL}] ${JOB_ID}"

  if openclaw cron run "${JOB_ID}" --expect-final --timeout "${TIMEOUT_MS}" 2>&1; then
    log "✅ [${NUM}/${TOTAL}] 완료"
    PASSED=$((PASSED + 1))
  else
    EXIT_CODE=$?
    log "❌ [${NUM}/${TOTAL}] 실패 (exit ${EXIT_CODE}) — 순차 실행 중단"
    FAILED=$((FAILED + 1))
    FAILED_JOBS+=("${JOB_ID}")
    break
  fi

  # job 사이 2초 대기 (gateway 부하 분산)
  [[ ${NUM} -lt ${TOTAL} ]] && sleep 2
done

log "=== 실행 완료: 성공 ${PASSED} / 실패 ${FAILED} / 전체 ${TOTAL} ==="

if [[ ${#FAILED_JOBS[@]} -gt 0 ]]; then
  log "실패 job: ${FAILED_JOBS[*]}"
  exit 1
fi
