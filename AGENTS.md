# Project Guide for AI agents

This Agents.md file provides comprehensive guidance for OpenAI Codex and other AI agents working with this codebase.

## Project Structure for OpenAI Codex Navigation

- `/actual-lab`: Source code of notebook that both computes and reports all results
  - `/task`: Description of the task we are working on here
    - `/task.md`: Actual description in most friendly format
  - `/main.dib`: .Net interactive notebook (F#), containing all computations and reports of the resulsts of the dask

## Hot to run project

`dotnet repl --run ./main.dib --exit-after-run --output-format trx`
