import { mkdirSync, writeFileSync } from 'node:fs';
import { dirname, join } from 'node:path';

const baseUrl = process.env.API_BASE_URL?.trim() || 'http://localhost:5169';
const outputPath = join(process.cwd(), 'dist/web/browser/config.json');

mkdirSync(dirname(outputPath), { recursive: true });
writeFileSync(outputPath, `${JSON.stringify({ api: { baseUrl } }, null, 2)}\n`);

console.log(`Wrote ${outputPath} with api.baseUrl=${baseUrl}`);
