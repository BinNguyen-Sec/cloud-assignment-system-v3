import { Route, Routes } from 'react-router-dom';
import { FoundationPreviewPage } from '../features/system/pages/FoundationPreviewPage';
import { NotFoundPage } from '../features/system/pages/NotFoundPage';

export function App() {
  return (
    <Routes>
      <Route path="/" element={<FoundationPreviewPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
