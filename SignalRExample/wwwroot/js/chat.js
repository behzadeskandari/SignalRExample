(function () {

    const connection = new signalR.HubConnectionBuilder().withUrl("/chathub").configureLogging(signalR.LogLevel.Information).build();


    async function start() {
        try {
            await connection.start();
            console.log("SignalR Connected");
        } catch (err) {
            console.log(err);
            setTimeout(start, 5000);
        }
    }


    connection.on("Log", function (message) {
        console.log(message);
    })


    connection.on("OnlineUsers", function (client) {
        console.log(client);
    })

    connection.invoke("SendMessage", 1,"Hello")

    start();

})();