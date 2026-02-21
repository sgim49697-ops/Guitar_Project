const express = require('express');
const WebSocket = require('ws');
const http = require('http');

const app = express();
const PORT = 8000;

// 튜너 HTML은 캐시 금지 (iPhone Safari 캐시 문제 방지)
app.get('/tuner', (req, res) => {
  res.set('Cache-Control', 'no-store');
  res.sendFile(__dirname + '/index.html');
});
app.get('/tuner/index.html', (req, res) => {
  res.set('Cache-Control', 'no-store');
  res.sendFile(__dirname + '/index.html');
});

// 정적 파일 제공 - 튜너 앱
app.use('/tuner', express.static(__dirname));

// Unity WebGL 파일 헤더 설정 미들웨어 (압축 + 비압축 모두 처리)
app.use('/practice', (req, res, next) => {
  const path = req.path;

  // Brotli 압축 파일 (.br)
  if (path.endsWith('.br')) {
    res.set('Content-Encoding', 'br');

    // 원본 파일 타입 감지 (.xxx.br 형태에서 확장자 추출)
    if (path.includes('.js.br')) {
      res.set('Content-Type', 'application/javascript; charset=utf-8');
    } else if (path.includes('.wasm.br')) {
      res.set('Content-Type', 'application/wasm');
    } else if (path.includes('.data.br')) {
      res.set('Content-Type', 'application/octet-stream');
    } else if (path.includes('.json.br')) {
      res.set('Content-Type', 'application/json; charset=utf-8');
    } else if (path.includes('.symbols.json.br')) {
      res.set('Content-Type', 'application/octet-stream');
    }
  }

  // Gzip 압축 파일 (.gz)
  else if (path.endsWith('.gz')) {
    res.set('Content-Encoding', 'gzip');

    if (path.includes('.js.gz')) {
      res.set('Content-Type', 'application/javascript; charset=utf-8');
    } else if (path.includes('.wasm.gz')) {
      res.set('Content-Type', 'application/wasm');
    } else if (path.includes('.data.gz')) {
      res.set('Content-Type', 'application/octet-stream');
    } else if (path.includes('.json.gz')) {
      res.set('Content-Type', 'application/json; charset=utf-8');
    }
  }

  // 압축되지 않은 파일들 (MIME 타입 명시)
  else {
    if (path.endsWith('.js')) {
      res.set('Content-Type', 'application/javascript; charset=utf-8');
    } else if (path.endsWith('.wasm')) {
      res.set('Content-Type', 'application/wasm');
    } else if (path.endsWith('.json')) {
      res.set('Content-Type', 'application/json; charset=utf-8');
    } else if (path.endsWith('.css')) {
      res.set('Content-Type', 'text/css; charset=utf-8');
    } else if (path.endsWith('.html')) {
      res.set('Content-Type', 'text/html; charset=utf-8');
    }
    // 이미지 파일
    else if (path.endsWith('.png')) {
      res.set('Content-Type', 'image/png');
    } else if (path.endsWith('.jpg') || path.endsWith('.jpeg')) {
      res.set('Content-Type', 'image/jpeg');
    } else if (path.endsWith('.ico')) {
      res.set('Content-Type', 'image/x-icon');
    } else if (path.endsWith('.svg')) {
      res.set('Content-Type', 'image/svg+xml');
    }
  }

  next();
});

// Unity WebGL 연습 앱 (Docker 볼륨 마운트)
app.use('/practice', express.static('/app/practice'));

// 루트 접근 시 튜너로 리다이렉트
app.get('/', (req, res) => {
  res.redirect('/tuner');
});

// HTTP 서버 생성
const server = http.createServer(app);

// WebSocket 서버 생성
const wss = new WebSocket.Server({ server });

// 연결된 클라이언트 저장
const clients = {
  input: null,    // iPhone (마이크 입력)
  display: null   // 컴퓨터 (결과 표시)
};

wss.on('connection', (ws, req) => {
  console.log('새 클라이언트 연결됨');

  ws.on('message', (message) => {
    try {
      const data = JSON.parse(message);

      // 클라이언트 타입 등록
      if (data.type === 'register') {
        if (data.role === 'input') {
          clients.input = ws;
          console.log('입력 클라이언트 등록 (iPhone)');
        } else if (data.role === 'display') {
          clients.display = ws;
          console.log('디스플레이 클라이언트 등록 (컴퓨터)');
        }
      }

      // 튜닝 데이터 전달 (input → display)
      if (data.type === 'tuning' && clients.display && clients.display.readyState === WebSocket.OPEN) {
        clients.display.send(JSON.stringify(data));
      }
    } catch (e) {
      console.error('메시지 처리 오류:', e);
    }
  });

  ws.on('close', () => {
    console.log('클라이언트 연결 종료');
    // 연결 종료된 클라이언트 제거
    if (clients.input === ws) clients.input = null;
    if (clients.display === ws) clients.display = null;
  });
});

server.listen(PORT, '0.0.0.0', () => {
  console.log('\n=================================');
  console.log('🎸 Guitar Project 서버 실행 중');
  console.log('=================================');
  console.log(`포트: ${PORT}`);
  console.log('\n🎵 튜너 앱:');
  console.log(`   http://localhost:${PORT}/tuner`);
  console.log('\n🎸 연습 앱 (Unity WebGL):');
  console.log(`   http://localhost:${PORT}/practice`);
  console.log('\n📱 iPhone 입력:');
  console.log(`   http://[컴퓨터IP]:${PORT}/tuner/input.html`);
  console.log('\n💻 컴퓨터 디스플레이:');
  console.log(`   http://localhost:${PORT}/tuner/display.html`);
  console.log('\n💡 컴퓨터 IP 확인: hostname -I');
  console.log('=================================\n');
});
