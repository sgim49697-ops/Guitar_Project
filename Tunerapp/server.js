const express = require('express');
const WebSocket = require('ws');
const path = require('path');
const http = require('http');

const app = express();
const PORT = 8000;

// 정적 파일 제공 - 튜너 앱
app.use('/tuner', express.static(__dirname));

// Unity WebGL 연습 앱 (빌드 후 활성화)
app.use('/practice', express.static(path.join(__dirname, '..', 'UnityApp', 'Builds', 'WebGL')));

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
