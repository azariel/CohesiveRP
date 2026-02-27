import styles from "./ChatSelectionComponent.module.css";
import { useEffect, useState, useRef  } from "react";
import { FaFilter  } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";

// Backend webapi
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SelectableChatsResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatsResponseDto";
import type { SelectableChatResponseDto } from "../../../../ResponsesDto/chatSelection/SelectableChatResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextChatType } from "../../../../store/SharedContextChat";

export default function ChatSelectionComponent() {
  const { setActiveModule } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const [chatDefinitions, setChatDefinitions] = useState<SelectableChatsResponseDto>();

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        const response:SelectableChatsResponseDto | null = await getFromServerApiAsync<SelectableChatsResponseDto>("api/chats");

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch all chats failed. [${serverApiException}] [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setChatDefinitions(response ?? []);
        console.log(`All chats fetched successfully.`);
      } catch (error) {
        console.error("Fetch error:", error);
      }
    };

    fetchData();
  }, []);
  
  const handleCreateNewChatClick = async () => {
    // TODO: load a screen to select the char, initial scenario, configuration, etc!
    const payload = {};
    const response:SelectableChatResponseDto | null = await postToServerApiAsync<SelectableChatResponseDto>("api/chats", payload);

    // add the newly created chat to state
    if(response) {
      setChatDefinitions(previousCollection => {
        if (!previousCollection) {
          return { chats: [response]} as SelectableChatsResponseDto;
        }
        return {
          ...previousCollection,// copy other fields
          chats: [...(previousCollection.chats || []), response],// add newly created chat on top of others
        };
      });


      console.log(`New chat created. Response: [${JSON.stringify(response)}].`);
    } else {
      console.error(`Failed to create a new chat. Response:[${response}] json: [${JSON.stringify(response)}].`);
    }
  };

  const handleSpecificChatClick = async (chat: SelectableChatResponseDto) => {

    let selectedChat = {
      chatId: chat.chatId,
      moduleName: "chat"
    } as SharedContextChatType;

    setActiveModule(selectedChat);
    console.log(`Chat selected -> Module [${JSON.stringify(selectedChat)}] selected.`);
  };

  return (
    <main className={styles.chatSelectionComponent}>
      <div className={styles.filterRow}>
        <FaFilter />
      </div>
      <div className={styles.chatsGridContainer}>
        <div>
            <div className={styles.chatAddNewChatContainer} onClick={async () => await handleCreateNewChatClick()}>
              <MdAddBox className={styles.chatAddNewChatBtn} />
            </div>
          </div>
        {chatDefinitions?.chats?.map((chat, index) => (
          <div key={index}>
            <label className={styles.chatCharNameLabel}>{chat.name}</label>
            <div className={styles.chatAvatarContainer} onClick={async () => await handleSpecificChatClick(chat)}>
              <img src="./dev/Seyrdis.png" alt="Avatar" />
            </div>
            <label className={styles.chatFootLabel}>{chat.lastActivityDateTime?.toDateString()}</label>
          </div>
        ))}
      </div>
    </main>
  );
}