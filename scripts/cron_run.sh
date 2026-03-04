#!/usr/bin/env bash
# cron_run.sh - job 이름으로 openclaw cron run 실행
# 사용법: bash scripts/cron_run.sh <job-name> [--expect-final]
#
# 예시:
#   bash scripts/cron_run.sh p2-a1a-songdata-schema
#   bash scripts/cron_run.sh p2-a1a-songdata-schema --expect-final

set -e

JOB_NAME="${1:?Usage: $0 <job-name> [--expect-final]}"
shift
EXTRA_ARGS="$@"

# openclaw cron list --json에서 이름으로 UUID 검색
UUID=$(openclaw cron list --json 2>/dev/null | python3 -c "
import json, sys
data = json.load(sys.stdin)
name = '$JOB_NAME'
for job in data.get('jobs', []):
    if job.get('name') == name:
        print(job['id'])
        break
")

if [ -z "$UUID" ]; then
  echo "❌ job 이름을 찾을 수 없음: $JOB_NAME"
  echo "등록된 job 목록:"
  openclaw cron list 2>/dev/null | awk 'NR>1 {print "  " $2}'
  exit 1
fi

echo "▶ 실행: $JOB_NAME (UUID: $UUID)"
openclaw cron run "$UUID" $EXTRA_ARGS
