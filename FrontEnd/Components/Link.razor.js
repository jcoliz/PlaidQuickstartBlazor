const linkPromise = (link_token) => {
    return new Promise(resolve => {

        var result = {
            ok: false,
            public_token: "**FAIL**",
            err: null,
            metadata: null
        };

        var handler = Plaid.create({
            token: link_token,
            onLoad: function () {
                // Optional, called when Link loads
            },

            onSuccess: function (public_token, metadata) {
                result.ok = true;
                result.public_token = public_token;
                result.metadata = metadata;
                resolve(result);
            },

            onExit: function (err, metadata) {
                // The user exited the Link flow.
                result.err = err;
                result.metadata = metadata;
                resolve(result);
            },

            onEvent: function (eventName, metadata) {
                // Optionally capture Link flow events, streamed through
                // this callback as your users connect an Item to Plaid.
            }

        });

        handler.open();
    })
}

export async function launch_link(link_token) {

    var outcome = await linkPromise(link_token);

    return JSON.stringify(outcome);
}