import './App.css';
import MainHeader from "./main/components/header/MainHeaderComponent";
import MainCenterBody from "./main/components/main/MainCenterComponent";
import MainLeftBody from './main/components/main/MainLeftComponent';
import MainRightBody from './main/components/main/MainRightComponent';

/* Store */
import { AppSharedStoreProvider } from './store/AppSharedStoreContext';

function App() {
  return (
    <>
    <AppSharedStoreProvider>
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
      </AppSharedStoreProvider>
    </>
  )
}

export default App
