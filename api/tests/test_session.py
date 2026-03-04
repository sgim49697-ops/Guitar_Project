from fastapi.testclient import TestClient
import pytest

from api import main as api_main


client = TestClient(api_main.app)


@pytest.fixture(autouse=True)
def reset_session_state():
    api_main.SESSIONS.clear()
    api_main.SESSION_HISTORY.clear()
    api_main.PRACTICE_LOGS.clear()
    api_main.ACTIVE_SESSION_ID = None
    yield
    api_main.SESSIONS.clear()
    api_main.SESSION_HISTORY.clear()
    api_main.PRACTICE_LOGS.clear()
    api_main.ACTIVE_SESSION_ID = None


def test_session_start_returns_session_id_and_initial_state():
    start_payload = {
        "songId": "fly-me-to-the-moon",
        "config": {
            "tempoRampBpm": 5,
            "loopCount": 2,
            "countInEnabled": True,
            "autoTransposeEnabled": True,
            "transposeSemitonesPerLoop": 5,
        },
    }

    r = client.post("/api/session/start", json=start_payload)
    assert r.status_code == 200

    body = r.json()
    assert "sessionId" in body
    session_id = body["sessionId"]

    state = client.get("/api/session/state")
    assert state.status_code == 200
    current = state.json()
    assert current["sessionId"] == session_id
    assert current["songId"] == "fly-me-to-the-moon"
    assert current["loopIndex"] == 0
    assert current["status"] == "active"
    assert current["currentBpm"] == 140
    assert current["currentKey"] == "C"


def test_loop_complete_applies_tempo_ramp_auto_transpose_and_completion():
    client.post(
        "/api/session/start",
        json={
            "songId": "fly-me-to-the-moon",
            "config": {
                "tempoRampBpm": 5,
                "loopCount": 2,
                "autoTransposeEnabled": True,
                "transposeSemitonesPerLoop": 5,
            },
        },
    )

    first = client.post("/api/session/loop-complete")
    assert first.status_code == 200
    body = first.json()
    assert body["loopIndex"] == 1
    assert body["currentBpm"] == 145
    assert body["currentKey"] == "F"  # C + 5 semitones
    assert body["status"] == "active"

    second = client.post("/api/session/loop-complete")
    assert second.status_code == 200
    body = second.json()
    assert body["loopIndex"] == 2
    assert body["currentBpm"] == 150
    assert body["currentKey"] == "Bb"  # F + 5 semitones
    assert body["status"] == "completed"


def test_end_session_saves_log_and_logs_endpoint_returns_it():
    start = client.post(
        "/api/session/start",
        json={
            "songId": "fly-me-to-the-moon",
            "config": {
                "tempoRampBpm": 5,
                "loopCount": 2,
                "autoTransposeEnabled": True,
                "transposeSemitonesPerLoop": 5,
            },
        },
    )
    session_id = start.json()["sessionId"]

    client.post("/api/session/loop-complete")
    client.post("/api/session/loop-complete")

    end = client.post("/api/session/end")
    assert end.status_code == 200
    log = end.json()
    assert log["sessionId"] == session_id
    assert log["songId"] == "fly-me-to-the-moon"
    assert log["totalLoops"] == 2
    assert log["finalBpm"] == 150
    assert log["finalKey"] == "Bb"

    logs = client.get("/api/session/logs")
    assert logs.status_code == 200
    items = logs.json()
    assert isinstance(items, list)
    assert len(items) == 1
    assert items[0]["sessionId"] == session_id


def test_state_without_active_session_returns_404():
    r = client.get("/api/session/state")
    assert r.status_code == 404
