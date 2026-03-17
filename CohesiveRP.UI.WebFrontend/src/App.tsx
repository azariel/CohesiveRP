import './App.css';
import MainHeader from "./main/components/header/MainHeaderComponent";
import MainCenterBody from "./main/components/main/MainCenterComponent";
import MainLeftBody from './main/components/main/MainLeftComponent';
import MainRightBody from './main/components/main/MainRightComponent';

/* Store */
import { AppSharedStoreProvider } from './store/AppSharedStoreContext';
import { MessagesStoreProvider } from "./store/MessagesStoreContext";

function App() {
  return (
    <>
    <AppSharedStoreProvider>
      <MessagesStoreProvider>
      <MainHeader />
        <div className='bodyControl'>
          <div className='bodyControlLeft'>
            <MainLeftBody />
          </div>
          <div className='bodyControlCenter'>
            <MainCenterBody />
          </div>
          <div className='bodyControlRight'>
            <MainRightBody />
          </div>
        </div>
        </MessagesStoreProvider>
      </AppSharedStoreProvider>
    </>
  )
}

export default App
