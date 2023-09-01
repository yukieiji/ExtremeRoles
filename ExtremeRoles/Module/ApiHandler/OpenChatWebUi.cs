using System;
using System.Linq;
using System.Net;
using System.Text;

using UnityEngine;

using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class OpenChatWebUi : IRequestHandler
{
	public Action<HttpListenerContext> Request => this.requestAction;

	private static string page =>
"""
<!DOCTYPE html>
<html>
    <head>
        <meta charset="UTF-8">
        <title>AmongUs Chat WebUI</title>
        <style>
            body, html {
                height: 100%;
                margin: 0;
                padding: 0;
            }
            #chat-container {
                width: 100vw;
                height: 100vh;
                display: flex;
                flex-direction: column;
                justify-content: space-around;
                align-items: center;
            }
                .message-item {
                    display: flex;
                    margin: 10px;
                }
                    .other {
                        justify-content: flex-start;
                    }
                    .my {
                        justify-content: flex-end;
                    }
                .item-body {
                    display: flex;
                    flex-direction: column;
                    margin-bottom: 10px;
                }
                    .other {
                        text-align: left;
                    }
                    .my {
                        text-align: right;
                    }
                .bubble {
                    background-color: #ffffff;
                    color: #000000;
                    border-radius: 10px;
                    padding: 10px;
                    max-width: 100%;
                    border: 2px solid #000000;
                    white-space: pre-wrap;
                }
                .sender {
                    font-weight: bold;
                    position: relative;
                }

                #chat-history {
                    width: 100%;
                    flex-grow: 1;
                    overflow-y: scroll;
                }
                #user-input-container {
                    height: 100px;
                    width: 100%;
                    bottom: 0;
                    background-color: #ffffff;
                    display: flex;
                    justify-content: space-between;
                    align-items: center;
                    margin: 10px;
                    box-shadow: 0px -1px 5px rgba(0, 0, 0, 0.2);
                }
                    #user-input {
                        width: 80%;
                        height: 50px;
                        margin: 10px;
                    }
                    #send-button {
                        width: 18%;
                        height: 40px;
                        background-color: #0074cc;
                        color: #ffffff;
                        border: none;
                        cursor: pointer;
                        margin: 10px;
                    }
        </style>
    </head>
<body>
    <div id="chat-container">
        <div id="chat-history"></div>
        <div id="user-input-container">
            <input type="text" id="user-input" placeholder="メッセージを入力">
            <button id="send-button">▶</button>
        </div>
    </div>

    <script>
        const chatHistory = document.getElementById("chat-history");
        const userInput = document.getElementById("user-input");
        const sendButton = document.getElementById("send-button");

        function resetMessage() {
            while( chatHistory.firstChild ){
                chatHistory.removeChild( chatHistory.firstChild );
            }
        }

        function addMessage(sender, message, isRight=false) {

            const messageItem = document.createElement("div");
            messageItem.className = isRight ?
                "message-item my":
                "message-item other";

            const itemBody = document.createElement("div");
            itemBody.className = isRight ?
                "item-body my":
                "item-body other";

            const senderElement = document.createElement("div");
            senderElement.className = "sender";
            senderElement.textContent = sender;

            const messageDiv = document.createElement("div");
            messageDiv.className = "bubble";
            messageDiv.textContent = message;

            itemBody.appendChild(senderElement);
            itemBody.appendChild(messageDiv);

            messageItem.appendChild(itemBody)

            chatHistory.appendChild(messageItem);
            chatHistory.scrollTop = chatHistory.scrollHeight;
        }

        async function handleUserInput() {
            const chat = userInput.value;
            const chatData = {
                "Body" : chat
            }
            const result = await fetch("http://localhost:57700/exr/chat/", {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(chatData),
            });

            if (result.ok)
            {
                userInput.value = "";
            }
        }

        var connection = new WebSocket("ws://localhost:57700/chat/ui/ws/");

        //接続通知
        connection.onopen = function(event) {
            addMessage("System", "SocketConnecting")
        };

        //エラー発生
        connection.onerror = function(error) {
            addMessage("System", error.data)
        };

        //メッセージ受信
        connection.onmessage = function(event) {
            const chat = JSON.parse(event.data);
            addMessage(chat.PlayerName, chat.Chat, chat.isRight)
        };

        //切断
        connection.onclose = function() {
            addMessage("System", "Disconnect")
        };

        sendButton.addEventListener("click", handleUserInput);

        userInput.addEventListener("keyup", function(event) {
            if (event.key === "Enter") {
                handleUserInput();
            }
        });
    </script>
</body>
</html>

""";

	private async void requestAction(HttpListenerContext context)
	{
		var response = context.Response;

		if (!DestroyableSingleton<HudManager>.InstanceExists ||
			FastDestroyableSingleton<HudManager>.Instance.Chat == null)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		IRequestHandler.SetStatusOK(response);

		response.ContentType = "text/html";
		response.ContentEncoding = Encoding.UTF8;
		byte[] buffer = Encoding.UTF8.GetBytes(page);
		response.Close(buffer, false);

		var chat = ChatWebUI.Instance;
		await chat.ConnectWs();
	}
}
