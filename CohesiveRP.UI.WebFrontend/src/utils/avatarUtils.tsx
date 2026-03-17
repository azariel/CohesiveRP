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


export {
    GetAvatarPathFromCharacterId,
    GetAvatarPathFromPersonaId
};