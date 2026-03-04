#!/bin/bash
# openclaw-sandbox.sh - sandbox 내에서 openclaw gateway를 호출하는 래퍼
#
# 동작 방식:
#   1. Python TCP 프록시: localhost:18789 → host.docker.internal:18789
#      (openclaw CLI는 ws://localhost 만 허용 → loopback 우회)
#   2. OPENCLAW_CONFIG_PATH로 sandbox 전용 config 주입
#   3. node v22로 openclaw.mjs 실행
#
# 사용법: openclaw cron list / openclaw cron run <uuid>

exec python3 - "$@" << 'PYEOF'
import socket, threading, subprocess, os, sys, time

TARGET_HOST = "host.docker.internal"
TARGET_PORT = 18789
LOCAL_PORT  = 18789

def forward(src, dst):
    try:
        while True:
            data = src.recv(4096)
            if not data:
                break
            dst.send(data)
    except Exception:
        pass
    finally:
        try: src.close()
        except Exception: pass
        try: dst.close()
        except Exception: pass

def handle_client(client):
    try:
        server = socket.create_connection((TARGET_HOST, TARGET_PORT), timeout=5)
        server.settimeout(None)
        threading.Thread(target=forward, args=(client, server), daemon=True).start()
        threading.Thread(target=forward, args=(server, client), daemon=True).start()
    except Exception:
        try: client.close()
        except Exception: pass

proxy = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
proxy.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
proxy.bind(("127.0.0.1", LOCAL_PORT))
proxy.listen(10)
proxy.settimeout(0.05)

env = dict(os.environ,
    # HOME 강제: agent exec이 HOME=/workspace로 실행해도 identity는 /home/sandbox/.openclaw/ 에서 읽음
    HOME="/home/sandbox",
    # config도 data dir(.openclaw/) 안에 위치 → openclaw가 동일 디렉토리에서 identity 탐색
    OPENCLAW_CONFIG_PATH="/home/sandbox/.openclaw/openclaw-sandbox.json",
    OPENCLAW_NO_RESPAWN="1",
    NODE_OPTIONS="--no-warnings",
)

# sys.argv[1:] → shell이 넘긴 "$@" (첫 arg는 '--')
args = sys.argv[1:]
if args and args[0] == "--":
    args = args[1:]

child = subprocess.Popen(
    ["node", "/home/sandbox/.openclaw-pkg/openclaw.mjs"] + args,
    env=env,
)

while child.poll() is None:
    try:
        client, _ = proxy.accept()
        threading.Thread(target=handle_client, args=(client,), daemon=True).start()
    except socket.timeout:
        pass
    except Exception:
        break

proxy.close()
sys.exit(child.returncode or 0)
PYEOF
