import styles from "./LorebookSelectionComponent.module.css";
import { useEffect, useState, useRef  } from "react";
import { FaFilter  } from "react-icons/fa";
import { ImSpinner2 } from "react-icons/im";
import { AiOutlineDisconnect } from "react-icons/ai";

// Backend webapi
import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { LorebooksResponseDto } from "../../../../ResponsesDto/lorebooks/LorebooksResponseDto";
import type { LorebookResponseDto } from "../../../../ResponsesDto/lorebooks/LorebookResponseDto";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import type { SharedContextLorebookType } from "../../../../store/SharedContextLorebookType";
import { GetAvatarPathFromLorebookId, GetFallbackEmpty } from "../../../../utils/avatarUtils";
import { GetLorebookNameFontSize } from "../../../../utils/fontSizeUtils";
import { MdAddBox } from "react-icons/md";
import type { SharedContextType } from "../../../../store/SharedContextType";
import type { Lorebook } from "../../../../ResponsesDto/lorebooks/BusinessObjects/Lorebook";

export default function LorebookSelectionComponent() {
  const { navigateTo } = sharedContext();
  const [isLoading, setIsLoading] = useState(true);
  const didComponentMountAlready = useRef(false);
  const newLorebookFileInputRef = useRef<HTMLInputElement | null>(null);
  const [lorebookDefinitions, setLorebookDefinitions] = useState<LorebooksResponseDto>();
  const [isNetworkDown, setIsNetworkDown] = useState(false);
  const [isImportingLorebook, setIsImportingLorebook] = useState(false);

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      setIsLoading(true);
      try {
        const response:LorebooksResponseDto | null = await getFromServerApiAsync<LorebooksResponseDto>("api/lorebooks");

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch all lorebooks failed. [${serverApiException}] [${JSON.stringify(serverApiException)}]`);
          setIsNetworkDown(true);
          setLorebookDefinitions({
            code: -1,
            lorebooks: []
          });

          return;
        }

        setLorebookDefinitions(response ?? []);
        console.log(`All lorebooks fetched successfully.`);
      } catch (error) {
        console.error("Fetch error:", error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, []);
  
  const handleCreateNewLorebookClick = async () => {
    let module = {
      moduleName: 'lorebooks'
    } as SharedContextType;

    navigateTo(module);
  };

  const handleSpecificLorebookClick = async (lorebook: Lorebook) => {
    setIsLoading(true);
    try {
      let selectedLorebook = {
        moduleName: "lorebookDetails",
        selectedLorebookId: lorebook?.lorebookId,
      } as SharedContextLorebookType;

      navigateTo(selectedLorebook);
    } finally {
      setIsLoading(false);
    }
};

const handleAddLorebookFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
  const file = event.target.files?.[0];
  if (!file)
    return;

  const formData = new FormData();
  formData.append("file", file);

  try {
    setIsImportingLorebook(true);
    const response = await postToServerApiAsync<LorebookResponseDto>("api/lorebooks/import", formData);

    let serverApiException = response as ServerApiExceptionResponseDto | null;
    if (!response || response.code != 200 || serverApiException?.message)
    {
      console.error(`Upload new lorebook failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
    }
    
    console.log(`Lorebook uploaded successfully.`);

    // Add the new character to the list
    setLorebookDefinitions((prev) => {
        const lorebookToAdd = response?.lorebook;

        if (!lorebookToAdd)
          return prev;

        // If there is no previous state, create the wrapper object
        if (!prev) {
          return {
            code: 200,
            lorebooks: [lorebookToAdd]
          } as LorebooksResponseDto;
        }

        // If state exists, spread the old state and add the new character to the array
        return {
          ...prev,
          lorebooks: [lorebookToAdd, ...(prev.lorebooks || [])]
        };
    });
  } catch (err) {
    console.error(err);
    // TODO: show err to user
  } finally {
    event.target.value = ""; // reset file input for future uploads
    setIsImportingLorebook(false);
  }
};

const handleAddLorebookClick = () => {
    newLorebookFileInputRef.current?.click();
  };

  return (
    <main className={styles.lorebookSelectionComponent}>
      <div className={styles.lorebooksHeader}>
        {isLoading ? (
          <ImSpinner2 className={ styles.headerLoadingSpinner } />
        ) : (
          isNetworkDown ? (
            <p/>
          ) : (
            <div className={styles.lorebooksHeaderTools}>
                <div className={styles.filterRow}>
                  <FaFilter />
                </div>
                <div className={styles.lorebooksToolsComponent}>
                  {isImportingLorebook ? (
                    <ImSpinner2 className={ styles.importingLorebookSpinner } />
                  ) : (
                    <div>
                      <MdAddBox className={styles.addNewLorebookIcon} onClick={handleAddLorebookClick} />
                      <input
                        type="file"
                        ref={newLorebookFileInputRef}
                        style={{ display: "none" }}
                        onChange={handleAddLorebookFileSelected}
                      />
                  </div>
                  )}
                </div>
              </div>
            )
          )}
      </div>
      <div className={styles.lorebooksGridContainer}>
        {isLoading ? (
          <div className={styles.lorebookAddNewLorebookContainer}>
            <ImSpinner2 className={styles.isLoadingSpinner} />
          </div>
        ) : (
          isNetworkDown ? (
            <div className={styles.lorebookAddNewLorebookContainerDisconnected}>
              <AiOutlineDisconnect className={styles.lorebookAddNewLorebookBtn} />
            </div>
          ) : (
            <div className={styles.lorebookAddNewLorebookContainer} onClick={async () => await handleCreateNewLorebookClick()}>
              <MdAddBox className={styles.lorebookAddNewLorebookBtn} />
            </div>
          )
        )}
        {isLoading || lorebookDefinitions?.lorebooks && lorebookDefinitions.lorebooks.length > 0 ? (
          lorebookDefinitions?.lorebooks?.map((element, index) => (
            <div key={index} className={styles.lorebookCard}>
              <label className={styles.lorebookCharNameLabel} style={{ fontSize: GetLorebookNameFontSize(element?.name ?? "") }}>{element?.name}</label>
              <div className={styles.lorebookAvatarContainer} onClick={async () => await handleSpecificLorebookClick(element)}>
                <img src={GetAvatarPathFromLorebookId(element?.lorebookId)} alt="Avatar" onError={(e) => {
                    e.currentTarget.onerror = null;
                    e.currentTarget.src = GetFallbackEmpty();
                  }} />
              </div>
              <label className={styles.lorebookFootLabel}>{element?.lastActivityDateTime?.toDateString() ?? ""}</label>
            </div>
          ))
        ) : (
          <div />
        )}
      </div>
    </main>
  );
}