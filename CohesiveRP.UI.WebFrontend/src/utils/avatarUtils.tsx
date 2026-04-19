import type { CharacterAvatar } from "../ResponsesDto/chat/BusinessObjects/CharacterAvatar";

const GetAvatarPathFromCharacterName = (characterName: string): string => {
    try {
      if(characterName === "") {
        return `./assets/empty-avatar.svg`;
      }
      
      return `./characters/${characterName.toLowerCase().trim()}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromPersonaId = (personaId: string): string => {
    try {
      if(personaId === "") {
        return `./assets/empty-avatar.svg`;
      }
      
      return `./personas/${personaId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromLorebookId = (lorebookId: string): string => {
    try {
      if(lorebookId === "") {
        return `./assets/empty-avatar.svg`;
      }
      
      return `./lorebooks/${lorebookId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromChatId = (chatId: string): string => {
    try {
      if(chatId === "") {
        return `./assets/empty-avatar.svg`;
      }
      
      return `./chats/${chatId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromChatIdAndAvatarId = (chatId: string, avatarId: string): string => {
    try {
      if(chatId === "") {
        return `./assets/empty-avatar.svg`;
      }
      
      return `./chats/${chatId}/${avatarId}.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromAvatarFilePath = (avatarFilePath: string): string => {
    try {
      if (avatarFilePath === "") {
        return `./assets/empty-avatar.svg`;
      }

      const normalized = avatarFilePath.toLowerCase().replace(/\\/g, "/").replace(/^\//, "");
      return `./${normalized}`;
    } catch (err) {
      return "";
    }
};

const getAvatarPathFromSourceAvatarDefinition = (avatarDefinition: CharacterAvatar): string => {
 try {
    if (!avatarDefinition || !avatarDefinition.name || !avatarDefinition.filePath) {
      return ""; // TODO: empty avatar? default? placeholder?
    }

    const normalized = avatarDefinition.filePath.toLowerCase().replace(/\\/g, "/").replace(/^\//, "");
    console.error(`FILE: ${normalized}`);
    return `./${normalized}`;
  } catch (err) {
    console.log(`Avatar not found for Character [${avatarDefinition?.name}], Outfit [${avatarDefinition?.outfit}], Expression [${avatarDefinition?.expression}], Error: ${err}`);
    return "";
  }
}

const getAvatarPathFromCharacterAvatarDefinition = (avatarDefinition: CharacterAvatar): string => {
  return getAvatarPathFromSourceAvatarDefinition(avatarDefinition);
};

const GetAvatarPathFromPersonaAvatarDefinition = (avatarDefinition: CharacterAvatar): string => {
  return getAvatarPathFromSourceAvatarDefinition(avatarDefinition);
};

export {
    GetAvatarPathFromCharacterName,
    GetAvatarPathFromPersonaId,
    GetAvatarPathFromLorebookId,
    GetAvatarPathFromChatId,
    GetAvatarPathFromChatIdAndAvatarId,
    GetAvatarPathFromAvatarFilePath,
    getAvatarPathFromCharacterAvatarDefinition,
    GetAvatarPathFromPersonaAvatarDefinition
};