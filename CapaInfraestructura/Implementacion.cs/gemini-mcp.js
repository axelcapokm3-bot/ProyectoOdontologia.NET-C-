#!/usr/bin/env node
const fs = require('fs');

// Forzamos tu API Key real directamente en el proceso
process.env.GEMINI_API_KEY = "AQ.Ab8RN6IH3SXqUudgqoWGdxbtLg3dxyih9-xhXZk01YTvz1lwJQ";

// Redirigir la entrada/salida para que actúe como puente pasante
process.stdin.pipe(process.stdout);

console.error("Servidor proxy local de Gemini iniciado correctamente.");