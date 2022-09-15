# Functional Tests

## How to run tests locally

1. Ensure you have an access_token configured (Functional tests do not test Link, currently)
1. Run the app in the background, e.g. using "startbg.ps1" from this directory
2. "dotnet test" from this directory

Note that if you haven't run tests in a while, the token has likely expired. You can fix your
existing token by loading the site in the browser (using the instance but up by startbg.ps1)
and choosing the "Fix Token" flow. Be sure to give access to ALL NINE accounts, or the tests
will fail.

## TODO: Run tests against deployed environment

This is not complete yet. When it's done, it will look something like...

1. Deploy ARM template as in Deploy.md
2. "dotnet test -s appservice.runsettings"
3. Remember to tear it down after!