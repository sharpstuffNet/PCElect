// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function getURLParameter(name) {
    var value = decodeURIComponent((RegExp(name + '=' + '(.+?)(&|$)').exec(location.search) || [, ""])[1]);
    return (value !== 'null') ? value : false;
}

// Write your Javascript code.
function Vote() {
    var vid = getURLParameter('VID');
    var vote = [];

    $.each($("input[name='vote']:checked"), function () {
        vote.push($(this).val());
    });

    var dov = vote.length == 3;
    if (vote.length < 3) {
        dov = confirm("Keine 3 Kreuze. Trotzdem Abschicken?");
    }
    if (vote.length >3) {
        dov = confirm("Mehr als 3 Kreuze. Wahl ungültig, trotzdem Abschicken?");
    }
   
    if (dov) {
        $.ajax({
            method: "POST",
            url: "CastVote",
            Accept: "application/json",
            contentType: "application/json",
            data: JSON.stringify({ VID: vid, Vote: vote }),
            processData: false
        })
            .done(function (msg) {
                alert("Voting Erfolgreich");
            }).fail(function () {
                //alert("Fehler");
            })
            .always(function () {
                document.location.href = "Thanks";
            });
    }
}
