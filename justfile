#!/usr/bin/env just --justfile

publish-dotnet:
    git symbolic-ref HEAD | cut -d/ -f3- | xargs -I % gh workflow run dotnet-publish.yaml -r %

publish-python:
    git symbolic-ref HEAD | cut -d/ -f3- | xargs -I % gh workflow run python-publish.yaml -r %

watch-dotnet:
    gh run list --workflow=dotnet-publish.yaml --limit 1
