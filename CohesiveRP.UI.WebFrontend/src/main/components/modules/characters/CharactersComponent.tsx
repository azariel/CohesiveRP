import styles from "./CharactersComponent.module.css";
import { useRef, useEffect, useState } from "react";
import { FaFilter  } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextType } from "../../../../store/SharedContextType";
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { CharacterResponseDto } from "../../../../ResponsesDto/characters/CharacterResponseDto";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { CharactersResponseDto } from "../../../../ResponsesDto/characters/CharactersResponseDto";

export default function CharactersComponent() {
  const { setActiveModule } = sharedContext();
  const didComponentMountAlready = useRef(false);
  const newCharacterFileInputRef = useRef<HTMLInputElement | null>(null);
  const [charactersResponse, setCharactersResponse] = useState<CharactersResponseDto | null>(null);

  useEffect(() => {
    if (didComponentMountAlready.current)
        return;
    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        const response: CharactersResponseDto | null = await getFromServerApiAsync<CharactersResponseDto>(`api/characters`);
        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch characters list failed. [${JSON.stringify(serverApiException)}]`);
          return;
        }

        console.log(`Characters list fetched successfully.`);
        setCharactersResponse(response);
      } catch (error) {
        console.error("Fetch characters list error:", error);
      }
    };
    fetchData();
  }, []);

  const handleSpecificCharacterClick = (moduleName: string) => {

    let module = {
      moduleName: moduleName
    } as SharedContextType;
    setActiveModule(module);
    console.log(`Character selected -> Module [${moduleName}] selected.`);
  };

  const handleAddCharacterClick = () => {
    newCharacterFileInputRef.current?.click();
  };

  const handleAddCharacterFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file)
      return;

    const formData = new FormData();
    formData.append("file", file);

    try {
      const response = await postToServerApiAsync<CharacterResponseDto>("api/characters", formData);

      let serverApiException = response as ServerApiExceptionResponseDto | null;
      if (!response || response.code != 200 || serverApiException?.message)
      {
        console.error(`Upload new character failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
      }
      
      console.log(`Character uploaded successfully.`);

      const newCharacter = response as CharacterResponseDto;

      // Add the new character to the list
      setCharactersResponse((prev) => {
        // If there is no previous state, create the wrapper object
        if (!prev) {
          return {
            code: 200,
            characters: [newCharacter.character]
          } as CharactersResponseDto;
        }

        // If state exists, spread the old state and add the new character to the array
        return {
          ...prev,
          characters: [...(prev.characters || []), newCharacter.character]
        };
    });
    } catch (err) {
      console.error(err);
      // TODO: show err to user
    } finally {
      event.target.value = ""; // reset file input for future uploads
    }
  };

  return (
    <main className={styles.charactersComponent}>
      <div className={styles.charactersHeader}>
        <div className={styles.filterRow}>
          <FaFilter />
        </div>
        <div className={styles.charactersToolsComponent}>
          <MdAddBox className={styles.addNewCharacterIcon} onClick={handleAddCharacterClick} />
          <input
            type="file"
            ref={newCharacterFileInputRef}
            style={{ display: "none" }}
            onChange={handleAddCharacterFileSelected}
          />
        </div>
      </div>
      <div className={styles.charactersGridContainer}>
        {charactersResponse?.characters?.map(character => (
          <div key={character.characterId} className={styles.characterContainer} onClick={() => handleSpecificCharacterClick("character")}>
            <div className={styles.characterAvatarContainer}>
              <img src={`./characters/${character.characterId}/avatar.png`} alt="dev/Placeholder.png" />
            </div>
            <div className={styles.characterInfoPanel}>
              <label className={styles.characterCharNameLabel}>{character.name}</label>
              <label className={styles.characterTagsLabel}>{!character.tags ? "" : character.tags.join(" / ")}</label>
              <label className={styles.characterDescriptionLabel}>
                {character.creatorNotes.length > 512 ? 
                  `${character.creatorNotes.substring(0, 512)}...` : 
                  character.creatorNotes}
              </label>
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}