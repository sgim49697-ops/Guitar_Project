from fastapi.testclient import TestClient
import pytest

from api import main as api_main


client = TestClient(api_main.app)


@pytest.fixture(autouse=True)
def reset_custom_songs():
    api_main.CUSTOM_SONGS.clear()
    yield
    api_main.CUSTOM_SONGS.clear()


def _valid_song_payload(**overrides):
    payload = {
        "title": "Test Tune",
        "composer": "Tester",
        "style": "Medium Swing",
        "key": "C",
        "bpm": 120,
        "timeSignature": "4/4",
        "chordProgression": ["| Cmaj7 | Dm7 G7 |"],
        "sections": [{"label": "A", "startIndex": 0}],
        "genre": "Jazz",
        "tags": ["test"],
    }
    payload.update(overrides)
    return payload


def test_get_songs_returns_5_seed_songs_and_valid_fields():
    r = client.get("/api/songs")
    assert r.status_code == 200

    body = r.json()
    assert body["total"] == 5
    assert len(body["songs"]) == 5

    required_fields = {
        "id",
        "title",
        "composer",
        "style",
        "key",
        "bpm",
        "timeSignature",
        "chordProgression",
        "sections",
        "genre",
        "tags",
    }

    for song in body["songs"]:
        assert required_fields.issubset(song.keys())


def test_get_song_by_id_and_404_for_missing_id():
    ok = client.get("/api/songs/fly-me-to-the-moon")
    assert ok.status_code == 200
    assert ok.json()["id"] == "fly-me-to-the-moon"

    not_found = client.get("/api/songs/does-not-exist")
    assert not_found.status_code == 404


def test_get_songs_with_genre_filter():
    r = client.get("/api/songs?genre=jazz")
    assert r.status_code == 200

    body = r.json()
    assert body["total"] == 5
    assert len(body["songs"]) == 5
    assert all(song["genre"].lower() == "jazz" for song in body["songs"])


def test_post_songs_returns_201_and_new_id():
    r = client.post("/api/songs", json=_valid_song_payload())
    assert r.status_code == 201

    created = r.json()
    assert isinstance(created["id"], str)
    assert created["id"]
    assert created["title"] == "Test Tune"


def test_put_song_updates_existing_song():
    created = client.post("/api/songs", json=_valid_song_payload()).json()
    song_id = created["id"]

    update_payload = _valid_song_payload(title="Updated Tune", bpm=150)
    update = client.put(f"/api/songs/{song_id}", json=update_payload)
    assert update.status_code == 200
    updated = update.json()
    assert updated["id"] == song_id
    assert updated["title"] == "Updated Tune"
    assert updated["bpm"] == 150


def test_delete_builtin_song_returns_403_forbidden():
    r = client.delete("/api/songs/autumn-leaves")
    assert r.status_code == 403


def test_delete_custom_song_returns_204():
    created = client.post("/api/songs", json=_valid_song_payload()).json()
    song_id = created["id"]

    deleted = client.delete(f"/api/songs/{song_id}")
    assert deleted.status_code == 204

    check = client.get(f"/api/songs/{song_id}")
    assert check.status_code == 404
