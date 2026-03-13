// import styles from "./PersonasSelectionComponent.module.css";
// import { useRef, useEffect, useState } from "react";
// import { FaFilter  } from "react-icons/fa";
// import { MdAddBox } from "react-icons/md";
// import { AiOutlineDisconnect } from "react-icons/ai";

// /* Store */
// import { sharedContext } from '../../../../store/AppSharedStoreContext';
// import { getFromServerApiAsync, postToServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
// import type { PersonaResponseDto } from "../../../../ResponsesDto/personas/PersonaResponseDto";
// import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
// import type { PersonasResponseDto } from "../../../../ResponsesDto/personas/PersonasResponseDto";
// import { GetAvatarPathFromPersonaId } from "../../../../utils/avatarUtils";
// import type { SharedContextPersonaType } from "../../../../store/SharedContextPersonaType";

// export default function PersonasSelectionComponent() {
//   const { setActiveModule } = sharedContext();
//   const didComponentMountAlready = useRef(false);
//   const newPersonaFileInputRef = useRef<HTMLInputElement | null>(null);
//   const [isNetworkDown, setIsNetworkDown] = useState(false);
//   const [personasResponse, setPersonasResponse] = useState<PersonasResponseDto | null>(null);

//   useEffect(() => {
//     if (didComponentMountAlready.current)
//         return;
//     didComponentMountAlready.current = true;

//     const fetchData = async () => {
//       try {
//         const response: PersonasResponseDto | null = await getFromServerApiAsync<PersonasResponseDto>(`api/personas`);
//         let serverApiException = response as ServerApiExceptionResponseDto | null;
//         if (!response || response.code != 200 || serverApiException?.message) {
//           console.error(`Call to fetch personas list failed. [${JSON.stringify(serverApiException)}]`);
//           setIsNetworkDown(true);
//           setPersonasResponse({
//             code : -1,
//             personas: []
//           });
//           return;
//         }

//         console.log(`Personas list fetched successfully.`);
//         setPersonasResponse(response);
//       } catch (error) {
//         console.error("Fetch personas list error:", error);
//       }
//     };
//     fetchData();
//   }, []);

//   const handleSpecificPersonaClick = (personaId: string) => {
//     let selectedPersona = {
//       selectedPersonaId: personaId,
//       moduleName: "personaDetails",
//     } as SharedContextPersonaType;
//     setActiveModule(selectedPersona);
//     console.log(`PersonaDetails selected -> Module personaDetails selected.`);
//   };

//   const handleAddPersonaClick = () => {
//     newPersonaFileInputRef.current?.click();
//   };

//   const handleAddPersonaFileSelected = async (event: React.ChangeEvent<HTMLInputElement>) => {
//     const file = event.target.files?.[0];
//     if (!file)
//       return;

//     const formData = new FormData();
//     formData.append("file", file);

//     try {
//       const response = await postToServerApiAsync<PersonaResponseDto>("api/personas", formData);

//       let serverApiException = response as ServerApiExceptionResponseDto | null;
//       if (!response || response.code != 200 || serverApiException?.message)
//       {
//         console.error(`Upload new persona failed. Error Code:[${response?.code}], Message: [${serverApiException?.message}], Message(Json): [${JSON.stringify(serverApiException?.message)}].`);
//       }
      
//       console.log(`Persona uploaded successfully.`);

//       const newPersona = response as PersonaResponseDto;

//       // Add the new persona to the list
//       setPersonasResponse((prev) => {
//         const charToAdd = newPersona.persona;

//         if (!charToAdd)
//           return prev;

//         // If there is no previous state, create the wrapper object
//         if (!prev) {
//           return {
//             code: 200,
//             personas: [charToAdd]
//           } as PersonasResponseDto;
//         }

//         // If state exists, spread the old state and add the new persona to the array
//         return {
//           ...prev,
//           personas: [charToAdd, ...(prev.personas || [])]
//         };
//     });
//     } catch (err) {
//       console.error(err);
//       // TODO: show err to user
//     } finally {
//       event.target.value = ""; // reset file input for future uploads
//     }
//   };

//   return (
//     <main className={styles.personasComponent}>
//       {isNetworkDown ? (
//           <div className={styles.networkDownContainer}>
//             <AiOutlineDisconnect className={styles.networkDownIcon} />
//             <label>CohesiveRP backend is unreachable</label>
//           </div>
//         ) : (
//         <div className={styles.personasMainContainer}>
//           <div className={styles.personasHeader}>
//             <div className={styles.filterRow}>
//               <FaFilter />
//             </div>
//             <div className={styles.personasToolsComponent}>
//               <MdAddBox className={styles.addNewPersonaIcon} onClick={handleAddPersonaClick} />
//                 <input
//                   type="file"
//                   ref={newPersonaFileInputRef}
//                   style={{ display: "none" }}
//                   onChange={handleAddPersonaFileSelected}
//               />
//             </div>
//           </div>
//           <div className={styles.personasGridContainer}>
//             {personasResponse?.personas?.map(persona => (
//               <div key={persona.personaId} className={styles.personaContainer} onClick={() => handleSpecificPersonaClick(persona.personaId)}>
//                 <div className={styles.personaAvatarContainer}>
//                   <img src={GetAvatarPathFromPersonaId(persona.personaId)} alt="dev/Placeholder.png" />
//                 </div>
//                 <div className={styles.personaInfoPanel}>
//                   <label className={styles.personaCharNameLabel}>{persona.name}</label>
//                   <label className={styles.personaTagsLabel}>{!persona.tags ? "" : persona.tags.join(" / ")}</label>
//                   <label className={styles.personaDescriptionLabel}>
//                     {persona.creatorNotes.length > 512 ? 
//                       `${persona.creatorNotes.substring(0, 512)}...` : 
//                       persona.creatorNotes}
//                   </label>
//                 </div>
//               </div>
//             ))}
//           </div>
//         </div>
//       )}
//     </main>
//   );
// }