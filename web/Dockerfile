# Etap 1: build
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .

# Accept build argument for mode
ARG VITE_MODE=production
ENV VITE_MODE=$VITE_MODE

RUN npm run build

# Etap 2: serwowanie statycznych plików
FROM node:20-alpine AS production
WORKDIR /app
RUN npm install -g serve
COPY --from=build /app/dist ./dist
EXPOSE 4173
CMD ["serve", "-s", "dist", "-l", "4173"]
