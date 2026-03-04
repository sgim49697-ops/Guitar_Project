from fastapi import FastAPI, HTTPException, Query, Response
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import Dict, List, Optional
from datetime import datetime, timezone
from uuid import UUID, uuid4

app = FastAPI(title="Guitar Practice AI API")

# CORS 설정 (Unity WebGL에서 접근 허용)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # 프로덕션에서는 특정 도메인만 허용
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==================== 데이터 모델 ====================

VALID_KEYS = {
    "C", "C#", "DB", "D", "D#", "EB", "E", "F", "F#", "GB",
    "G", "G#", "AB", "A", "A#", "BB", "B",
    "CMINOR", "C#MINOR", "DBMINOR", "DMINOR", "D#MINOR", "EBMINOR", "EMINOR",
    "FMINOR", "F#MINOR", "GBMINOR", "GMINOR", "G#MINOR", "ABMINOR", "AMINOR",
    "A#MINOR", "BBMINOR", "BMINOR",
}


class SectionMarker(BaseModel):
    label: str
    startIndex: int


class SongBase(BaseModel):
    title: str
    composer: str
    style: str
    key: str
    bpm: int
    timeSignature: str
    chordProgression: List[str]
    sections: List[SectionMarker] = []
    genre: str
    tags: List[str] = []


class SongData(SongBase):
    id: str


class SongsResponse(BaseModel):
    songs: List[SongData]
    total: int


class ChordExplainRequest(BaseModel):
    chord_name: str


class ChordExplainResponse(BaseModel):
    chord_name: str
    explanation: str
    tips: List[str]
    similar_chords: List[str]


# ==================== Song 데이터 ====================

SONGS: Dict[str, SongData] = {
    "autumn-leaves": SongData(
        id="autumn-leaves",
        title="Autumn Leaves",
        composer="Joseph Kosma",
        style="Medium Swing",
        key="Bb",
        bpm=120,
        timeSignature="4/4",
        chordProgression=[
            "| Cm7 F7 | Bbmaj7 Ebmaj7 |",
            "| Am7b5 D7 | Gm6 |",
            "| Cm7 F7 | Bbmaj7 Ebmaj7 |",
            "| Am7b5 D7 | Gm6 |",
        ],
        sections=[SectionMarker(label="A", startIndex=0)],
        genre="Jazz",
        tags=["standard", "swing"],
    ),
    "blue-bossa": SongData(
        id="blue-bossa",
        title="Blue Bossa",
        composer="Kenny Dorham",
        style="Bossa Nova",
        key="C-minor",
        bpm=130,
        timeSignature="4/4",
        chordProgression=[
            "| Cm7 | Cm7 | Fm7 | Fm7 |",
            "| Dm7b5 G7 | Cm7 | Cm7 |",
        ],
        sections=[SectionMarker(label="A", startIndex=0)],
        genre="Jazz",
        tags=["latin", "bossa"],
    ),
    "fly-me-to-the-moon": SongData(
        id="fly-me-to-the-moon",
        title="Fly Me to the Moon",
        composer="Bart Howard",
        style="Medium Swing",
        key="C",
        bpm=140,
        timeSignature="4/4",
        chordProgression=[
            "| Am7 D7 | Gmaj7 Cmaj7 |",
            "| F#m7b5 B7 | Em7 A7 |",
            "| Dm7 G7 | Cmaj7 A7 |",
            "| Dm7 G7 | Cmaj7 |",
        ],
        sections=[SectionMarker(label="A", startIndex=0)],
        genre="Jazz",
        tags=["standard", "vocal"],
    ),
    "summertime": SongData(
        id="summertime",
        title="Summertime",
        composer="George Gershwin",
        style="Slow Swing",
        key="A-minor",
        bpm=70,
        timeSignature="4/4",
        chordProgression=[
            "| Am6 | Am6 | Dm6 | Dm6 |",
            "| Am6 | E7 | Am6 E7 | Am6 |",
        ],
        sections=[SectionMarker(label="A", startIndex=0)],
        genre="Jazz",
        tags=["ballad", "standard"],
    ),
    "all-of-me": SongData(
        id="all-of-me",
        title="All of Me",
        composer="Gerald Marks / Seymour Simons",
        style="Medium Swing",
        key="C",
        bpm=160,
        timeSignature="4/4",
        chordProgression=[
            "| C6 | E7 | A7 | Dm7 |",
            "| E7 | Am7 | D7 | G7 |",
            "| C6 A7 | Dm7 G7 | C6 A7 | Dm7 G7 |",
        ],
        sections=[SectionMarker(label="A", startIndex=0)],
        genre="Jazz",
        tags=["swing", "standard"],
    ),
}

CUSTOM_SONGS: Dict[str, SongData] = {}

CHROMATIC_KEYS = ['C', 'Db', 'D', 'Eb', 'E', 'F', 'F#', 'G', 'Ab', 'A', 'Bb', 'B']


class PracticeConfig(BaseModel):
    tempoRampBpm: int = 0
    loopCount: int = -1
    countInEnabled: bool = True
    autoTransposeEnabled: bool = False
    transposeSemitonesPerLoop: int = 0


class StartSessionRequest(BaseModel):
    songId: str
    config: PracticeConfig = Field(default_factory=PracticeConfig)


class StartSessionResponse(BaseModel):
    sessionId: UUID


class PracticeSessionState(BaseModel):
    sessionId: UUID
    songId: str
    currentKey: str
    currentBpm: int
    loopIndex: int = 0
    status: str = "active"
    config: PracticeConfig
    startTime: str


class PracticeLog(BaseModel):
    sessionId: UUID
    songId: str
    totalLoops: int
    startTime: str
    endTime: str
    finalBpm: int
    finalKey: str


SESSIONS: Dict[UUID, PracticeSessionState] = {}
ACTIVE_SESSION_ID: Optional[UUID] = None
SESSION_HISTORY: Dict[UUID, PracticeSessionState] = {}
PRACTICE_LOGS: List[PracticeLog] = []



def _normalize_key(value: str) -> str:
    return value.replace(" ", "").replace("-", "").upper()


def _validate_song_input(song: SongBase) -> None:
    if not song.chordProgression:
        raise HTTPException(status_code=400, detail="chordProgression must not be empty")

    normalized = _normalize_key(song.key)
    if normalized not in VALID_KEYS:
        raise HTTPException(status_code=400, detail=f"Invalid key: {song.key}")


def _transpose_key_chromatic(current_key: str, semitones: int) -> str:
    if semitones == 0:
        return current_key

    normalized = current_key.strip()
    if not normalized:
        return current_key

    major_map = {
        "C": "C", "B#": "C",
        "DB": "Db", "C#": "Db",
        "D": "D",
        "EB": "Eb", "D#": "Eb",
        "E": "E", "FB": "E",
        "F": "F", "E#": "F",
        "GB": "F#", "F#": "F#",
        "G": "G",
        "AB": "Ab", "G#": "Ab",
        "A": "A",
        "BB": "Bb", "A#": "Bb",
        "B": "B", "CB": "B",
    }

    is_minor = False
    root = normalized

    lowered = normalized.lower()
    if lowered.endswith('-minor'):
        is_minor = True
        root = normalized[:-6]
    elif lowered.endswith('minor'):
        is_minor = True
        root = normalized[:-5]
    elif lowered.endswith('m') and len(normalized) > 1:
        is_minor = True
        root = normalized[:-1]

    root = root.strip()
    key = major_map.get(root.upper())
    if key is None:
        return current_key

    current_idx = CHROMATIC_KEYS.index(key)
    next_idx = (current_idx + semitones) % len(CHROMATIC_KEYS)
    transposed = CHROMATIC_KEYS[next_idx]

    return f"{transposed}-minor" if is_minor else transposed


# ==================== 헬스체크 ====================

@app.get("/")
async def root():
    return {
        "service": "Guitar Practice AI API",
        "status": "running",
        "version": "1.0.0"
    }


@app.get("/health")
async def health_check():
    return {"status": "healthy"}


# ==================== Song API ====================

@app.get("/api/songs", response_model=SongsResponse)
async def list_songs(
    genre: Optional[str] = Query(default=None),
    style: Optional[str] = Query(default=None),
    key: Optional[str] = Query(default=None),
):
    songs = list(SONGS.values()) + list(CUSTOM_SONGS.values())

    if genre:
        songs = [s for s in songs if s.genre.lower() == genre.lower()]
    if style:
        songs = [s for s in songs if s.style.lower() == style.lower()]
    if key:
        songs = [s for s in songs if s.key.lower() == key.lower()]

    return SongsResponse(songs=songs, total=len(songs))


@app.get("/api/songs/{song_id}", response_model=SongData)
async def get_song(song_id: str):
    song = SONGS.get(song_id) or CUSTOM_SONGS.get(song_id)
    if not song:
        raise HTTPException(status_code=404, detail="Song not found")
    return song


@app.post("/api/songs", response_model=SongData, status_code=201)
async def create_song(song: SongBase):
    _validate_song_input(song)

    new_id = str(uuid4())
    new_song = SongData(id=new_id, **song.model_dump())
    CUSTOM_SONGS[new_id] = new_song
    return new_song


@app.put("/api/songs/{song_id}", response_model=SongData)
async def update_song(song_id: str, song: SongBase):
    existing = SONGS.get(song_id) or CUSTOM_SONGS.get(song_id)
    if not existing:
        raise HTTPException(status_code=404, detail="Song not found")

    _validate_song_input(song)

    updated = SongData(id=song_id, **song.model_dump())

    if song_id in SONGS:
        SONGS[song_id] = updated
    else:
        CUSTOM_SONGS[song_id] = updated

    return updated


@app.delete("/api/songs/{song_id}", status_code=204)
async def delete_song(song_id: str):
    if song_id in SONGS:
        raise HTTPException(status_code=403, detail="Built-in songs cannot be deleted")

    if song_id not in CUSTOM_SONGS:
        raise HTTPException(status_code=404, detail="Song not found")

    del CUSTOM_SONGS[song_id]
    return Response(status_code=204)


@app.post("/api/session/start", response_model=StartSessionResponse)
async def start_session(req: StartSessionRequest):
    global ACTIVE_SESSION_ID

    song = SONGS.get(req.songId) or CUSTOM_SONGS.get(req.songId)
    if song is None:
        raise HTTPException(status_code=404, detail="Song not found")

    # 동시 세션은 1개만 허용: 기존 활성 세션 자동 종료
    if ACTIVE_SESSION_ID is not None and ACTIVE_SESSION_ID in SESSIONS:
        previous = SESSIONS[ACTIVE_SESSION_ID].model_copy(update={"status": "completed"})
        SESSION_HISTORY[ACTIVE_SESSION_ID] = previous
        del SESSIONS[ACTIVE_SESSION_ID]
        ACTIVE_SESSION_ID = None

    new_session_id = uuid4()
    session = PracticeSessionState(
        sessionId=new_session_id,
        songId=song.id,
        currentKey=song.key,
        currentBpm=song.bpm,
        loopIndex=0,
        status="active",
        config=req.config,
        startTime=datetime.now(timezone.utc).isoformat(),
    )

    SESSIONS[new_session_id] = session
    ACTIVE_SESSION_ID = new_session_id
    return StartSessionResponse(sessionId=new_session_id)


@app.post("/api/session/loop-complete", response_model=PracticeSessionState)
async def complete_session_loop():
    if ACTIVE_SESSION_ID is None or ACTIVE_SESSION_ID not in SESSIONS:
        raise HTTPException(status_code=404, detail="Active session not found")

    current_session = SESSIONS[ACTIVE_SESSION_ID]

    if current_session.status == "completed":
        return current_session

    current_session.loopIndex += 1

    tempo_ramp = current_session.config.tempoRampBpm
    if tempo_ramp:
        current_session.currentBpm += tempo_ramp

    if current_session.config.autoTransposeEnabled:
        semitones = current_session.config.transposeSemitonesPerLoop
        current_session.currentKey = _transpose_key_chromatic(current_session.currentKey, semitones)

    loop_count = current_session.config.loopCount
    if loop_count != -1 and current_session.loopIndex >= loop_count:
        current_session.status = "completed"

    return current_session


@app.get("/api/session/state", response_model=PracticeSessionState)
async def get_current_session_state():
    if ACTIVE_SESSION_ID is None or ACTIVE_SESSION_ID not in SESSIONS:
        raise HTTPException(status_code=404, detail="Active session not found")

    current_session = SESSIONS[ACTIVE_SESSION_ID]
    if current_session.status != "active":
        raise HTTPException(status_code=404, detail="Active session not found")
    return current_session


@app.get("/api/session/state/{session_id}", response_model=PracticeSessionState)
async def get_session_state(session_id: str):
    try:
        session_uuid = UUID(session_id)
    except ValueError as exc:
        raise HTTPException(status_code=400, detail="Invalid session id") from exc

    if session_uuid in SESSIONS:
        return SESSIONS[session_uuid]

    archived = SESSION_HISTORY.get(session_uuid)
    if archived is None:
        raise HTTPException(status_code=404, detail="Session not found")
    return archived


@app.post("/api/session/end", response_model=PracticeLog)
async def end_session():
    global ACTIVE_SESSION_ID

    if ACTIVE_SESSION_ID is None or ACTIVE_SESSION_ID not in SESSIONS:
        raise HTTPException(status_code=404, detail="Active session not found")

    current_session = SESSIONS[ACTIVE_SESSION_ID]
    end_time = datetime.now(timezone.utc).isoformat()
    finalized = current_session.model_copy(update={"status": "completed"})

    log = PracticeLog(
        sessionId=finalized.sessionId,
        songId=finalized.songId,
        totalLoops=finalized.loopIndex,
        startTime=finalized.startTime,
        endTime=end_time,
        finalBpm=finalized.currentBpm,
        finalKey=finalized.currentKey,
    )

    SESSION_HISTORY[ACTIVE_SESSION_ID] = finalized
    PRACTICE_LOGS.append(log)
    del SESSIONS[ACTIVE_SESSION_ID]
    ACTIVE_SESSION_ID = None

    return log


@app.get("/api/session/logs", response_model=List[PracticeLog])
async def list_practice_logs():
    return list(reversed(PRACTICE_LOGS[-50:]))




# ==================== AI 엔드포인트 ====================

@app.post("/api/explain-chord", response_model=ChordExplainResponse)
async def explain_chord(req: ChordExplainRequest):
    """
    코드 설명 생성 (추후 LLM 연동)
    현재는 하드코딩된 데이터 반환
    """
    # TODO: OpenAI, Claude, 또는 로컬 LLM 연동

    chord_data = {
        "C": {
            "explanation": "C Major는 가장 기본적인 코드 중 하나입니다. 밝고 안정적인 사운드를 가지고 있어서 수많은 곡에서 사용됩니다.",
            "tips": [
                "검지, 중지, 약지를 사용합니다",
                "6번 줄(굵은 E)은 뮤트하세요",
                "손가락을 프렛에 가깝게 두면 깨끗한 소리가 납니다"
            ],
            "similar_chords": ["Am", "Em", "G"]
        },
        "G": {
            "explanation": "G Major는 개방현을 많이 사용해서 풍성한 소리가 나는 코드입니다. C 코드와 자주 함께 쓰입니다.",
            "tips": [
                "중지, 약지, 소지를 사용합니다",
                "6번 줄 3프렛을 정확히 눌러야 합니다",
                "모든 줄을 울려야 풍성한 소리가 납니다"
            ],
            "similar_chords": ["Em", "C", "D"]
        },
        "Am": {
            "explanation": "A Minor는 약간 슬픈 느낌의 코드입니다. C 코드와 손가락 모양이 비슷해서 전환이 쉽습니다.",
            "tips": [
                "검지, 중지, 약지를 사용합니다",
                "6번 줄은 치지 않습니다",
                "C 코드에서 중지만 한 칸 아래로 이동하면 됩니다"
            ],
            "similar_chords": ["C", "Em", "Dm"]
        },
        "F": {
            "explanation": "F Major는 바레 코드의 대표격입니다. 초보자에게는 어렵지만, 마스터하면 많은 곡을 연주할 수 있습니다.",
            "tips": [
                "검지로 1프렛을 전부 눌러야 합니다 (바레)",
                "손목을 약간 아래로 내리면 힘이 덜 듭니다",
                "간소화 버전(xx3211)부터 연습하세요"
            ],
            "similar_chords": ["Dm", "Am", "C"]
        },
        "D": {
            "explanation": "D Major는 밝고 명랑한 사운드의 코드입니다. 상대적으로 쉽고, 많은 곡에서 사용됩니다.",
            "tips": [
                "검지, 중지, 약지를 사용합니다",
                "5번, 6번 줄은 치지 않습니다",
                "삼각형 모양으로 손가락을 배치하세요"
            ],
            "similar_chords": ["A", "G", "Em"]
        }
    }

    chord_name = req.chord_name.upper()
    if chord_name not in chord_data:
        # 기본 응답
        return ChordExplainResponse(
            chord_name=chord_name,
            explanation=f"{chord_name} 코드에 대한 상세 정보는 준비 중입니다.",
            tips=["코드를 천천히 연습하세요", "메트로놈을 사용하세요"],
            similar_chords=[]
        )

    data = chord_data[chord_name]
    return ChordExplainResponse(
        chord_name=chord_name,
        explanation=data["explanation"],
        tips=data["tips"],
        similar_chords=data["similar_chords"]
    )


# ==================== 사용자 진행도 ====================

class ProgressUpdate(BaseModel):
    user_id: str
    session_data: dict  # {"chord": "C", "duration": 300, "success_rate": 0.85}


@app.post("/api/progress")
async def update_progress(progress: ProgressUpdate):
    """
    사용자 연습 진행도 저장 (MongoDB 연동 예정)
    """
    # TODO: MongoDB에 저장
    return {"status": "saved", "user_id": progress.user_id}


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(app, host="0.0.0.0", port=8080)
