import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import { lazy, Suspense } from 'react';

const Chat    = lazy(() => import('./components/modules/chat/ChatComponent'));
const ChatsSelection    = lazy(() => import('./components/modules/chatsSelection/ChatSelectionComponent'));
const CharactersSelection    = lazy(() => import('./components/modules/charactersSelection/CharactersComponent'));
const Settings    = lazy(() => import('./components/modules/settings/SettingsComponent'));

export default function App() {
  return (
    <BrowserRouter>
      <nav>
        <Link to="/chat">Chat</Link>
        <Link to="/chatsSelection">ChatsSelection</Link>
        <Link to="/charactersSelection">CharactersSelection</Link>
        <Link to="/settings">Settings</Link>
      </nav>

      <Suspense fallback={<div>Loading...</div>}>
        <Routes>
          <Route path="/chat"        element={<Chat />} />
          <Route path="/chatsSelection"   element={<ChatsSelection />} />
          <Route path="/charactersSelection" element={<CharactersSelection />} />
          <Route path="/settings" element={<Settings />} />
        </Routes>
      </Suspense>
    </BrowserRouter>
  );
}