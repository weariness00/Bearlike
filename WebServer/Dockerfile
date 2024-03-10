# Stage 1: Build stage using Node.js
FROM node:16-alpine AS builder

# 작업 디렉토리 설정
WORKDIR /app

# 의존성 파일 복사 및 설치
COPY package*.json ./
RUN npm install

# 애플리케이션 코드 복사
COPY . .

# 필요한 경우, 빌드 스크립트 실행 (이 부분은 애플리케이션에 따라 다를 수 있습니다)
# RUN npm run build

# Stage 2: Run stage using Node.js
FROM node:16-alpine

# 작업 디렉토리 설정
WORKDIR /app

# Non-root 사용자 생성
RUN addgroup -S appgroup && adduser -S appuser -G appgroup

# 빌드 단계에서 생성된 파일 복사
# COPY --from=builder /app/build ./build (빌드 결과물이 있는 경우에만 사용)

# 필요한 파일만 복사
COPY --from=builder /app/package*.json ./
COPY --from=builder /app/ .

# 프로덕션 의존성 설치
RUN npm install --only=production

# Non-root 사용자로 전환
USER appuser

# 앱 시작 명령어
CMD ["node", "server.js"]
