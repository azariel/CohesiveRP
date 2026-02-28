import styles from "./SettingsComponent.module.css";
import { useEffect, useState, useRef  } from "react";

// Backend webapi
import { getFromServerApiAsync } from "../../../../utils/http/HttpRequestHelper";
import type { ServerApiExceptionResponseDto } from "../../../../ResponsesDto/Exceptions/ServerApiExceptionResponseDto";
import type { SettingsResponseDto } from "../../../../ResponsesDto/settings/SettingsResponseDto"

/* Store */
//import type { SharedContextSettings } from "../../../../store/SharedContextSettings";

export default function SettingsComponent() {
  //sharedContext<SharedContextSettings>();
  const didComponentMountAlready = useRef(false);
  const [settings, setSettings] = useState<SettingsResponseDto | undefined>(undefined);

  useEffect(() => {
    if (didComponentMountAlready.current)
      return;

    didComponentMountAlready.current = true;

    const fetchData = async () => {
      try {
        const response:SettingsResponseDto | null = await getFromServerApiAsync<SettingsResponseDto>("api/settings");

        let serverApiException = response as ServerApiExceptionResponseDto | null;
        if(!response || response.code != 200 || serverApiException?.message) {
          console.error(`Call to fetch settings failed. [${serverApiException}] [${JSON.stringify(serverApiException)}]`);
          return;
        }

        setSettings(response);
        console.log(`Settings fetched successfully.`);
      } catch (error) {
        console.error("Fetch error:", error);
      }
    };

    fetchData();
  }, [setSettings]);

  return (
    <main className={styles.settingsComponent}>
      
    </main>
  );
}