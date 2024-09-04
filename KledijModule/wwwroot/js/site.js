/// Onderstaande is voor error handling via htmx. Even testen zonder want het sukkelt een beetje met speciale tekens zoals ë, ö, ...

$(function () {
    document.body.addEventListener('htmx:afterRequest', function (evt) {
        // Custom logic to execute after an HTMX request completes
        console.log("HTMX request completed:", evt.detail);

        // You can access details of the event, such as evt.detail.successful, evt.detail.failed, etc.
        const errorTarget = document.getElementById("ajax-alert")
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


/*Jquery global error handling zodat ajax calls automatisch mooi error handlen\*/


/**
 * 
 * 
 *  Gebruik $(document).on("ajaxError", handler) om globale AJAX-fouten af te handelen.
 Gebruik $(document).on("ajaxSuccess", handler) voor het afhandelen van succesvolle AJAX-aanroepen.
 Gebruik $(document).on("ajaxComplete", handler) om zowel geslaagde als mislukte AJAX-aanroepen te verwerken.
 Je kunt de responscode verkrijgen via de `jqXHR` parameter in de handler van ajaxComplete.

 Voorbeeld:
 $(document).on("ajaxError", function (event, jqXHR, ajaxOptions, thrownError) {
     alert("Er is een AJAX-fout opgetreden!");
 });

 $(document).on("ajaxSuccess", function (event, jqXHR, ajaxOptions) {
     console.log("AJAX-aanroep geslaagd");
 });

 $(document).on("ajaxComplete", function (event, jqXHR, ajaxOptions) {
     alert("AJAX-aanroep voltooid met responscode " + jqXHR.status);
 });
 * 
 * 
 */

/*https://api.jquery.com/category/ajax/*/

$(document).on("ajaxError", function (event, jqXHR, ajaxOptions, thrownError) {
    console.warn("Global AJAX error", { jqXHR, ajaxOptions, thrownError });

    const errorTarget = document.getElementById("ajax-alert");

    if (jqXHR.status >= 400 && jqXHR.status < 500) {
        // Client-side error (4xx)
        errorTarget.innerText = `Client error: ${jqXHR.responseText}`;
    } else if (jqXHR.status >= 500) {
        // Server-side error (5xx)
        errorTarget.innerText = `Server error: ${jqXHR.responseText}`;
    } else {
        // Network or unspecified error
        errorTarget.innerText = "Unexpected error, check your connection and try to refresh the page.";
    }
    errorTarget.classList.remove("d-none");
});

$(document).on("ajaxSuccess", function (event, jqXHR, ajaxOptions) {
    console.log("Global AJAX success", { jqXHR, ajaxOptions });

    const errorTarget = document.getElementById("ajax-alert");
    // Verwijder eventuele vorige foutmeldingen bij succes
    errorTarget.classList.add("d-none");
    errorTarget.innerText = "";
});

$(document).on("ajaxComplete", function (event, jqXHR, ajaxOptions) {
    console.log("AJAX request completed with response code:", jqXHR.status);
});