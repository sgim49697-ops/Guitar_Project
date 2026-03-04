#!/usr/bin/env bash
# migrate_openclaw_to_docker.sh
# 호스트 OpenClaw 데몬 → Docker 완전 이전

set -e
cd "$(dirname "$0")/.."

echo "=== OpenClaw Docker 마이그레이션 ==="
echo ""

# 1. 호스트 openclaw 데몬 중지
echo "[1/4] 호스트 openclaw 데몬 중지..."
if pgrep -f "openclaw.*daemon\|openclaw.*gateway" > /dev/null 2>&1; then
  pkill -f "openclaw.*daemon" 2>/dev/null || true
  pkill -f "openclaw.*gateway" 2>/dev/null || true
  sleep 2
  echo "  ✅ 호스트 데몬 중지 완료"
else
  echo "  ℹ️  실행 중인 호스트 데몬 없음"
fi

# 2. openclaw-sandbox 이미지 사전 pull (선택)
echo ""
echo "[2/4] OpenClaw 이미지 pull..."
docker pull ghcr.io/openclaw/openclaw:latest
echo "  ✅ 이미지 준비 완료"

# 3. Gateway bind 설정을 lan으로 변경
echo ""
echo "[3/4] Gateway bind 설정 확인..."
OPENCLAW_JSON="$HOME/.openclaw/openclaw.json"
if [ -f "$OPENCLAW_JSON" ]; then
  python3 - <<'PYEOF'
import json, sys
path = f"{__import__('os').environ['HOME']}/.openclaw/openclaw.json"
with open(path) as f:
    d = json.load(f)

gw = d.setdefault('gateway', {})
if gw.get('bind') == 'loopback':
    gw['bind'] = 'lan'
    with open(path, 'w') as f:
        json.dump(d, f, indent=2)
    print("  ✅ gateway.bind: loopback → lan 변경")
else:
    print(f"  ℹ️  gateway.bind 현재값: {gw.get('bind', 'unset')} (변경 불필요)")
PYEOF
fi

# 4. Docker 컨테이너 시작
echo ""
echo "[4/4] Docker 컨테이너 시작..."
docker compose up -d openclaw-gateway
echo ""
echo "=== 완료 ==="
echo ""
echo "상태 확인:"
echo "  docker compose ps"
echo "  docker compose logs -f openclaw-gateway"
echo ""
echo "CLI 사용:"
echo "  docker compose run --rm --profile cli openclaw-cli cron list"
echo "  docker compose run --rm --profile cli openclaw-cli cron run p2-a1a-songdata-schema"
echo ""
echo "Control UI:"
echo "  http://localhost:18789/#token=${OPENCLAW_GATEWAY_TOKEN:-<토큰 확인 필요>}"
