using System;
using System.Net;
using System.Text;

using ExtremeRoles.Helper;
using ExtremeRoles.Module.Interface;
using ExtremeRoles.Performance;

namespace ExtremeRoles.Module.ApiHandler;

public sealed class OpenChatWebUi : IRequestHandler
{
	public const string SystemUser = "@EXR_MOD~SYSTEM";
	public const string RemoveOldChat = "@SYSTEM_REMOVE_OLDCHAT";

	public Action<HttpListenerContext> Request => this.requestAction;

	private const string page =
"""
<!DOCTYPE html>
<html>
    <head>
        <meta charset="UTF-8">
        <title>AmongUs Chat WebUI</title>
        <style>
            body, html {
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
            <input
				required
				type="text"
				id="user-input"
				maxlength="120"
				placeholder="|INPUT_MESSAGE|">
            <button id="send-button">▶</button>
        </div>
    </div>

    <script>
        const chatHistory = document.getElementById("chat-history");
        const userInput = document.getElementById("user-input");
        const sendButton = document.getElementById("send-button");

		function removeOld() {
            if( chatHistory.firstChild ){
                chatHistory.removeChild( chatHistory.firstChild );
            }
        }

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

            messageItem.appendChild(itemBody);

            chatHistory.appendChild(messageItem);
            chatHistory.scrollTop = chatHistory.scrollHeight;
        }

        async function handleUserInput() {
            const chat = userInput.value;

			if (chat.match(/["$%&'()\*\+\-\.,\/:;<=>@\[\\\]^_`{|}~]/gi))
			{
				addMessage("|SYSTEM_MESSAGE|", "|INVALID_CHAR_IN_MESSAGE|", true);
				userInput.value = "";
				return;
			}

			const chatData = {
                "Body" : chat
            }
            const result = await fetch("|POST_URL|", {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(chatData),
            });

            if (result.ok) {
                userInput.value = "";
            }
        }

        var connection = new WebSocket("ws://|SOCKET_URL|");

        //接続通知
        connection.onopen = function(event) {
            addMessage("|SYSTEM_MESSAGE|", "|ESTABLISH_CONNECT_MESSAGE|");
        };

        //エラー発生
        connection.onerror = function(error) {
            addMessage("|SYSTEM_MESSAGE|", error.data);
        };

        //メッセージ受信
        connection.onmessage = function(event) {
			if (event.data === null || event.data === "") {
				resetMessage();
				return;
			}

			const chat = JSON.parse(event.data);
			const playerName = chat.PlayerName;
			const chatBody = chat.Chat;

			if (playerName === "|SYSTEM_USER|") {
				if (chatBody === "|CMD_REMOVE_OLD_CHAT|") {
					removeOld();
				}
				return;
			}
			addMessage(chat.PlayerName, chat.Chat, chat.isRight);
        };

        //切断
        connection.onclose = function() {
            addMessage("|SYSTEM_MESSAGE|", "|DISCONNECT_MESSAGE|");
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

		if (!HudManager.InstanceExists ||
			HudManager.Instance.Chat == null)
		{
			response.StatusCode = (int)HttpStatusCode.PreconditionFailed;
			response.Abort();
			return;
		}

		IRequestHandler.SetStatusOK(response);

		response.ContentType = "text/html";
		response.ContentEncoding = Encoding.UTF8;

		string postChatPath = PostChat.Path;
		string socketUrl = ChatWebUI.SocketUrl;

		string showPage = page
			.Replace("|SYSTEM_USER|", SystemUser)
			.Replace("|CMD_REMOVE_OLD_CHAT|", RemoveOldChat)
			.Replace("|ESTABLISH_CONNECT_MESSAGE|", Tr.GetString("ConectSocketMessage"))
			.Replace("|DISCONNECT_MESSAGE|", Tr.GetString("DisconectAmongUsMessage"))
			.Replace("|SYSTEM_MESSAGE|", Tr.GetString("SystemMessage"))
			.Replace("|INPUT_MESSAGE|", Tr.GetString("InputMessage"))
			.Replace("|INVALID_CHAR_IN_MESSAGE|", Tr.GetString("InvalidCharMessage"))
			.Replace("|POST_URL|", $"{ApiServer.Url}{postChatPath}")
			.Replace("|SOCKET_URL|", socketUrl.Replace("http://", ""));

		byte[] buffer = Encoding.UTF8.GetBytes(showPage);
		response.Close(buffer, false);

		var chat = ChatWebUI.Instance;
		await chat.ConnectWs();
	}
}
