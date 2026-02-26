import styles from "./CharactersComponent.module.css";
import { FaFilter  } from "react-icons/fa";
import { MdAddBox } from "react-icons/md";

/* Store */
import { sharedContext } from '../../../../store/AppSharedStoreContext';
import { useRef } from "react";

export default function CharactersComponent() {
  const { setActiveModule } = sharedContext();
  const newCharacterFileInputRef = useRef<HTMLInputElement | null>(null);

  const handleSpecificCharacterClick = (moduleName: string) => {
    setActiveModule(moduleName);
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
      const response = await fetch("/api/upload", {
        method: "POST",
        body: formData,
      });

      if (!response.ok)
        throw new Error("Upload failed");
      
      const data = await response.json();
      console.log("Upload success:", data);
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
        {Array.from({ length: 16 }).map((_, index) => (
          <div className={styles.characterContainer} onClick={() => handleSpecificCharacterClick("character")}>
            <div
              key={index}
              className={styles.characterAvatarContainer}
            >
              <img src="./dev/Seyrdis.png" alt="Avatar" />
            </div>
            <div className={styles.characterInfoPanel}>
              <label className={styles.characterCharNameLabel}>char name {index}</label>
              <label className={styles.characterTagsLabel}>Angst Humor Drama Fantasy Adventure Isekai Magic Horror Combat Suspense</label>
              <label className={styles.characterDescriptionLabel}>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Aenean cursus, lorem at luctus interdum, nisi risus volutpat ex, at dignissim nulla magna id dui. Pellentesque a dolor eleifend, congue orci elementum, tempor turpis. Praesent pretium a justo ut pretium. Morbi placerat quis massa ac euismod. Nunc facilisis vel enim sit amet finibus. Nulla aliquam odio vitae lacus mattis, at placerat purus elementum. Nulla facilisi. Ut laoreet suscipit metus nec elementum. Maecenas semper ullamcorper tortor, id vestibulum dolor egestas euismod. In vel fringilla ex. Sed tristique commodo interdum. Sed venenatis sodales justo vel egestas. </label>
            </div>
          </div>
        ))}
      </div>
    </main>
  );
}