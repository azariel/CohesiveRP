const GetAvatarPathFromCharacterId = (characterId: string): string => {
    try {
      if(characterId === "") {
        return `./dev/Placeholder.png`;
      }
      
      return `./characters/${characterId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromPersonaId = (personaId: string): string => {
    try {
      if(personaId === "") {
        return `./dev/Placeholder.png`;
      }
      
      return `./personas/${personaId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromLorebookId = (lorebookId: string): string => {
    try {
      if(lorebookId === "") {
        return `./dev/Placeholder.png`;
      }
      
      return `./lorebooks/${lorebookId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromChatId = (chatId: string): string => {
    try {
      if(chatId === "") {
        return `./dev/Placeholder.png`;
      }
      
      return `./chats/${chatId}/avatar.png`;
  }catch(err){
    return "";
  }
};

const GetAvatarPathFromChatIdAndAvatarId = (chatId: string, avatarId: string): string => {
    try {
      if(chatId === "") {
        return `./dev/Placeholder.png`;
      }
      
      return `./chats/${chatId}/${avatarId}.png`;
  }catch(err){
    return "";
  }
};

export {
    GetAvatarPathFromCharacterId,
    GetAvatarPathFromPersonaId,
    GetAvatarPathFromLorebookId,
    GetAvatarPathFromChatId,
    GetAvatarPathFromChatIdAndAvatarId
};