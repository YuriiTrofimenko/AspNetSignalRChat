var viewModel = {
    chatMessages: ko.observableArray([]),
    chatUsername: ko.observable("")
}
viewModel.newMessage = {
    username: ko.computed(function () { return viewModel.chatUsername() }),
    text: ko.observable("")
}
viewModel.resizeChat = function () {
    $("#console").scrollTop($("#console")[0].scrollHeight);
};

$(document).ready(function () {
    ko.applyBindings(viewModel);
});

//Отправка нового сообщения на сервер
$("#pushbtn").click(function () {
    $.ajax({
        url: "http://localhost:5049/api/chat/",
        data: JSON.stringify(ko.mapping.toJS(viewModel.newMessage)),
        cache: false,
        type: 'POST',
        dataType: "json",
        contentType: 'application/json; charset=utf-8'
    });
    viewModel.newMessage.text('');
    $("#push").val('');
});

//Получение нового сообщения из WebSocket
if (!!window.EventSource) {
    var source = new EventSource('http://localhost:5049/api/chat/');
    source.addEventListener('message', function (e) {
        console.log("message");
        //console.log(e);
        var json = JSON.parse(e.data);
        viewModel.chatMessages.push(json);
    }, false);
    source.addEventListener('open', function (e) {
        console.log("open!");
    }, false);
    source.addEventListener('error', function (e) {
        if (e.readyState == EventSource.CLOSED) {
            console.log("error!");
        }
    }, false);
} else {
    // not supported!
    //fallback to something else
    console.log('!!window.EventSource is not supported. Fallback to something else');
}

