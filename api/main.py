from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from typing import List
import os

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

class ChordExplainRequest(BaseModel):
    chord_name: str

class ChordExplainResponse(BaseModel):
    chord_name: str
    explanation: str
    tips: List[str]
    similar_chords: List[str]

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
