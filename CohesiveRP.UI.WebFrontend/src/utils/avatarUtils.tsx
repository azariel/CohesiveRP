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


export {
    GetAvatarPathFromCharacterId
};