/// Onderstaande is voor error handling via htmx. Even testen zonder want het sukkelt een beetje met speciale tekens zoals ë, ö, ...

document.body.addEventListener('htmx:afterRequest', function (evt) {
    // Custom logic to execute after an HTMX request completes
    console.log("HTMX request completed:", evt.detail);

    // You can access details of the event, such as evt.detail.successful, evt.detail.failed, etc.
    const errorTarget = document.getElementById("htmx-alert")
    if (evt.detail.successful) {
        // Successful request, clear out alert
        errorTarget.classList.add("d-none");
        errorTarget.innerText = "";
    } else if (evt.detail.failed && evt.detail.xhr) {
        // Server error with response contents, equivalent to htmx:responseError
        console.warn("Server error", evt.detail)
        const xhr = evt.detail.xhr;
        errorTarget.innerText = `${xhr.responseText}`;
        errorTarget.classList.remove("d-none")
    } else {
        // Unspecified failure, usually caused by network error
        console.error("Unexpected htmx error", evt.detail)
        errorTarget.innerText = "Unexpected error, check your connection and try to refresh the page.";
        errorTarget.classList.remove("d-none")
    }
});

/*https://khalidabuhakmeh.com/htmx-requests-with-aspnet-core-anti-forgery-tokens*/
document.addEventListener("htmx:configRequest", (evt) => {
    let httpVerb = evt.detail.verb.toUpperCase();
    if (httpVerb === 'GET') return;

    let antiForgery = htmx.config.antiForgery;

    if (antiForgery) {

        // already specified on form, short circuit
        if (evt.detail.parameters[antiForgery.formFieldName])
            return;

        if (antiForgery.headerName) {
            evt.detail.headers[antiForgery.headerName]
                = antiForgery.requestToken;
        } else {
            evt.detail.parameters[antiForgery.formFieldName]
                = antiForgery.requestToken;
        }
    }
});