import { defineConfig } from 'orval';

export default defineConfig({
  api: {
    input: {
      target: process.env.OPENAPI_INPUT || 'http://localhost:5000/api/openapi/v1.json'
    },
    output: {
      mode: 'tags-split',
      target: 'src/api/generated',
      client: 'axios',
      fileExtension: '.ts',
      override: {
        enumGenerationType: 'enum',
        mutator: {
          path: 'src/api/axios-instance.ts',
          name: 'axiosInstance'
        }
      },
      // zod: {
      //   enabled: false
      // }
    }
  }
});
