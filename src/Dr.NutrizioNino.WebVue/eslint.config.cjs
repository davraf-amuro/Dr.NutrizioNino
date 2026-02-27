const vue = require('eslint-plugin-vue')
const tsParser = require('@typescript-eslint/parser')

module.exports = [
  {
    ignores: ['node_modules/**', 'dist/**', 'coverage/**']
  },
  ...vue.configs['flat/essential'],
  {
    files: ['**/*.{ts,tsx,mts,cts}'],
    languageOptions: {
      parser: tsParser,
      parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module'
      }
    }
  },
  {
    files: ['**/*.vue'],
    languageOptions: {
      parserOptions: {
        parser: tsParser
      }
    }
  }
]
