# Functional Tests

## How to run tests locally

1. Ensure you have an access_token configured (Functional tests do not test Link, currently)
1. Run the app in the background, e.g. using "startbg.ps1" from this directory
2. "dotnet test" from this directory

## TODO: Run tests against deployed environment

This is not complete yet. When it's done, it will look something like...

1. Deploy ARM template as in Deploy.md
2. "dotnet test -s appservice.runsettings"
3. Remember to tear it down after!