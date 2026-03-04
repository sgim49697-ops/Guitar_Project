# register_pipeline_jobs.py - guitar-pipeline.cron.json 의 모든 job을 openclaw에 등록

import json
import re
import subprocess
import sys
from pathlib import Path

CRON_JSON = Path(__file__).parent.parent / "guitar-pipeline.cron.json"


def strip_js_comments(text: str) -> str:
    """// 주석 제거 (JSON5 → JSON 변환)"""
    # 문자열 리터럴 안의 // 는 보존하기 위해 줄 단위로 처리
    lines = []
    for line in text.splitlines():
        # 문자열 밖의 // 주석 제거
        stripped = re.sub(r'(?<!:)//.*$', '', line).rstrip()
        lines.append(stripped)
    # trailing comma before ] or } 제거
    text = '\n'.join(lines)
    text = re.sub(r',\s*(\n\s*[}\]])', r'\1', text)
    return text


def register_job(job: dict, dry_run: bool = False) -> bool:
    job_id   = job.get("id", "")
    label    = job.get("label", job_id)
    prompt   = job.get("prompt", "")
    target   = job.get("sessionTarget", "isolated")
    announce = job.get("delivery", {}).get("mode") == "announce"

    # manual job: --cron "0 0 29 2 *" = 2월 29일(윤년, 4년에 1회)만 자동실행
    # → 사실상 수동 트리거 전용. force-run 후 삭제되지 않음 (recurring cron은 auto-delete 없음)
    cmd = [
        "openclaw", "cron", "add",
        "--name",         job_id,
        "--description",  label,
        "--cron",         "0 0 29 2 *",
        "--message",      prompt,
        "--session",      target,
    ]
    if announce:
        cmd.append("--announce")

    if dry_run:
        print(f"  [DRY] {' '.join(cmd[:6])} ... (prompt omitted)")
        return True

    result = subprocess.run(cmd, capture_output=True, text=True)
    if result.returncode == 0:
        print(f"  ✅ {job_id}")
        return True
    else:
        print(f"  ❌ {job_id}: {result.stderr.strip()}")
        return False


def main():
    dry_run = "--dry-run" in sys.argv
    phase_filter = None
    for arg in sys.argv[1:]:
        if arg.startswith("--phase="):
            phase_filter = arg.split("=")[1]

    raw = CRON_JSON.read_text(encoding="utf-8")
    clean = strip_js_comments(raw)
    data = json.loads(clean)

    jobs = data["cron"]["jobs"]
    if phase_filter:
        jobs = [j for j in jobs if j.get("phase") == phase_filter]

    print(f"{'[DRY RUN] ' if dry_run else ''}등록할 job: {len(jobs)}개")
    if phase_filter:
        print(f"Phase 필터: {phase_filter}")
    print()

    ok = fail = 0
    for job in jobs:
        if register_job(job, dry_run=dry_run):
            ok += 1
        else:
            fail += 1

    print(f"\n완료: {ok}개 성공, {fail}개 실패")
    if not dry_run and ok > 0:
        print("\n등록된 job 목록:")
        subprocess.run(["openclaw", "cron", "list"])


if __name__ == "__main__":
    main()
