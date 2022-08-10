function ShowLoadingModal() {
    $('#myLoader').modal('show');
}

function HideLoadingModal() {
    $('#myLoader').modal('hide');
}

function ShowSucessModal(messages, onclose) {
    var ul = $("<ul></ul>");
    for (var i = 0; i < messages.length; i++) {

        var li = $("<li></li>");
        li.html(messages[i]);
        ul.append(li);
    }

    $('#igmodalSucesso').unbind('hidden.bs.modal');

    if (onclose != null) {
        $('#igmodalSucesso').on('hidden.bs.modal', function () {
            onclose();
        });
    } else {
        $('#igmodalSucesso').on('hidden.bs.modal', false);
    }

    $("#igmodalSucesso .modal-body").html(ul);
    $("#igmodalSucesso").modal("show");
}

function ShowErrorModal(messages, onclose) {
    var ul = $("<ul></ul>");
    for (var i = 0; i < messages.length; i++) {

        var li = $("<li></li>");
        li.html(messages[i]);
        ul.append(li);
    }

    $('#igmodalErro').unbind('hidden.bs.modal');

    if (onclose != null) {
        $('#igmodalErro').on('hidden.bs.modal', function () {
            onclose();
        });
    } else {
        $('#igmodalErro').on('hidden.bs.modal', false);
    }

    $("#igmodalErro .modal-body").html(ul);
    $("#igmodalErro").modal("show");
}

function ShowConfirm(callbackfunction, title, message) {

    $("#shared-button-confirm").prop("onclick", null).off("click");

    $("#shared-title-confirm").html(title);
    $("#shared-mensagem-confirm").html(message);

    $("#shared-button-confirm").click(function () {
        callbackfunction();
    });

    $("#shared-modal-confirm").modal("show");
}