import styles from "./CharacterSheetInstancesSelectionComponent.module.css";
import { useEffect, useRef, useState } from "react";
import { AiOutlineDisconnect } from "react-icons/ai";
import { ImSpinner2 } from "react-icons/im";

import { getFromServerApiAsync } from "../../../../../utils/http/HttpRequestHelper";
import type { CharactersResponseDto } from "../../../../../ResponsesDto/characters/CharactersResponseDto";
import type { CharacterResponse } from "../../../../../ResponsesDto/characters/CharacterResponse";
import type { ServerApiExceptionResponseDto } from "../../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import { GetAvatarPathFromCharacterName, GetFallbackEmpty } from "../../../../../utils/avatarUtils";

interface Props {
  chatId: string;
  onSelectCharacter: (characterId: string, characterName: string) => void;
}

export default function CharacterSheetInstancesSelectionComponent({ chatId, onSelectCharacter }: Props) {
  const didMount = useRef(false);

  const [isLoading, setIsLoading] = useState(true);
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [characters, setCharacters] = useState<CharacterResponse[]>([]);

  useEffect(() => {
    if (didMount.current) return;
    didMount.current = true;

    const fetchData = async () => {
      try {
        setIsLoading(true);

        const response = await getFromServerApiAsync<CharactersResponseDto>(`api/chats/${chatId}/characters`);
        const ex = response as ServerApiExceptionResponseDto | null;
        if (!response || response.code != 200 || ex?.message) {
          console.error(`Fetch characters for chat failed. [${JSON.stringify(ex)}]`);
          setIsNetworkDown(true);
          return;
        }

        const candidates = response.characters ?? [];
        setCharacters(candidates.filter((c): c is CharacterResponse => c !== null));
      } catch (error) {
        console.error("Fetch character sheet instances list error:", error);
        setIsNetworkDown(true);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [chatId]);

  return (
    <main className={styles.instancesComponent}>
      {isNetworkDown ? (
        <div className={styles.networkDownContainer}>
          <AiOutlineDisconnect className={styles.networkDownIcon} />
          <label>CohesiveRP backend is unreachable</label>
        </div>
      ) : isLoading ? (
        <ImSpinner2 className={styles.loadingSpinner} />
      ) : (
        <div className={styles.instancesGridContainer}>
          {characters.length === 0 ? (
            <div className={styles.emptyState}>
              No character sheet instances are available for this chat yet.
            </div>
          ) : (
            characters.map((character) => (
              <div
                key={character.characterId}
                className={styles.characterContainer}
                onClick={() => onSelectCharacter(character.characterId, character.name)}
              >
                <div className={styles.characterAvatarContainer}>
                  <img
                    src={GetAvatarPathFromCharacterName(character.name)}
                    alt="Avatar"
                    onError={(e) => {
                      e.currentTarget.onerror = null;
                      e.currentTarget.src = GetFallbackEmpty();
                    }}
                  />
                </div>
                <div className={styles.characterInfoPanel}>
                  <label className={styles.characterNameLabel}>{character.name}</label>
                  <label className={styles.characterTagsLabel}>
                    {character.tags?.join(" / ") ?? ""}
                  </label>
                </div>
              </div>
            ))
          )}
        </div>
      )}
    </main>
  );
}