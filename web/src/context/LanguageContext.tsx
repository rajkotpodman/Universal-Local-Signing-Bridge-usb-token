import React, { createContext, useContext, useState, useEffect } from 'react';

export type Language = 'en' | 'gu';

interface Translations {
  [key: string]: {
    en: string;
    gu: string;
  };
}

export const translations: Translations = {
  // Navigation
  dashboard: { en: 'Dashboard', gu: 'ડેશબોર્ડ (Dashboard)' },
  certificates: { en: 'Certificates', gu: 'પ્રમાણપત્રો (DSC)' },
  providers: { en: 'Providers & Hardware', gu: 'પ્રોવાઇડર્સ અને ટોકન' },
  sign: { en: 'Signing Studio', gu: 'ડિજિટલ સહી (Studio)' },
  diagnostics: { en: 'Browser Diagnostics', gu: 'સિસ્ટમ તપાસ' },
  security: { en: 'Security Policy', gu: 'સુરક્ષા નિયમો' },
  audit: { en: 'Audit Logs', gu: 'ઓડિટ લોગ્સ' },
  settings: { en: 'Settings', gu: 'સેટિંગ્સ' },
  about: { en: 'About & Help', gu: 'પરિચય અને મદદ' },

  // TopBar
  bridgeTitle: { en: 'Universal Local Signing Bridge', gu: 'યુનિવર્સલ લોકલ સાઈનિંગ બ્રિજ' },
  serviceRunning: { en: 'SERVICE: Running', gu: 'સેવા: ચાલુ છે' },
  apiAvailable: { en: 'API: Available', gu: 'API: તૈયાર' },
  sync: { en: 'Sync', gu: 'રીફ્રેશ (Sync)' },
  quickHelp: { en: 'How to Use', gu: 'ઉપયોગ કેવી રીતે કરવો?' },

  // Dashboard & 1-Click Signer
  oneClickTitle: { en: '1-Click Quick PDF Signer', gu: '૧-ક્લિક ઝડપી પીડીએફ સહી (Quick Sign)' },
  oneClickSub: { en: 'Drop any PDF file below to digitally sign with your hardware security token.', gu: 'કોઈપણ પીડીએફ ફાઇલ અહીં મૂકો અને તમારા USB ટોકનથી તરત જ ડિજિટલ સહી મેળવો.' },
  selectOrDrop: { en: 'Choose a PDF file or Drag & Drop here', gu: 'પીડીએફ ફાઇલ પસંદ કરો અથવા અહીં ખેંચીને મૂકો' },
  selectCertLabel: { en: 'Signing Certificate / Token', gu: 'સહી કરવાનું પ્રમાણપત્ર / ટોકન' },
  signingReason: { en: 'Signature Reason / Statement', gu: 'સહી કરવાનું કારણ' },
  signAndDownload: { en: 'Sign Document with Token', gu: 'ટોકન વડે દસ્તાવેજ પર સહી કરો' },
  signedSuccess: { en: 'Document Successfully Signed!', gu: 'દસ્તાવેજ પર સફળતાપૂર્વક સહી થઈ ગઈ!' },
  downloadSignedPdf: { en: 'Download Signed PDF', gu: 'સહી થયેલ PDF ડાઉનલોડ કરો' },

  // Hardware Status
  hardwareActive: { en: 'PHYSICAL HARDWARE TOKEN ATTACHED', gu: 'અસલી હાર્ડવેર ટોકન જોડાયેલ છે' },
  noHardware: { en: 'NO HARDWARE TOKEN ATTACHED', gu: 'કોઈ હાર્ડવેર ટોકન જોડાયેલ નથી' },
  hardwareSubActive: { en: 'Security token connected and ready for digital signing.', gu: 'સુરક્ષા ટોકન જોડાયેલ છે અને સહી કરવા માટે સંપૂર્ણ સજ્જ છે.' },
  hardwareSubNone: { en: 'Please plug your USB DSC token (Aladdin, ePass, ProxKey) into any USB port.', gu: 'કૃપા કરીને તમારું USB ટોકન (Aladdin, ePass, ProxKey) કમ્પ્યુટરમાં લગાવો.' },

  // Metrics
  serviceCard: { en: 'SERVICE', gu: 'મુખ્ય સેવા' },
  apiCard: { en: 'REST API', gu: 'REST API' },
  providersCard: { en: 'ACTIVE PROVIDERS', gu: 'સક્રિય પ્રોવાઇડર્સ' },
  certificatesCard: { en: 'CERTIFICATES', gu: 'કુલ પ્રમાણપત્રો' },
  smartCardsCard: { en: 'SMART CARDS', gu: 'સ્માર્ટ કાર્ડ્સ' },
  operationsCard: { en: 'TOTAL SIGNATURES', gu: 'કુલ સહીઓ' }
};

interface LanguageContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  t: (key: string) => string;
}

const LanguageContext = createContext<LanguageContextType>({
  language: 'en',
  setLanguage: () => {},
  t: (key: string) => key
});

export const LanguageProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [language, setLanguageState] = useState<Language>(() => {
    return (localStorage.getItem('bridge_lang') as Language) || 'en';
  });

  const setLanguage = (lang: Language) => {
    setLanguageState(lang);
    localStorage.setItem('bridge_lang', lang);
  };

  const t = (key: string): string => {
    if (translations[key]) {
      return translations[key][language] || translations[key].en;
    }
    return key;
  };

  return (
    <LanguageContext.Provider value={{ language, setLanguage, t }}>
      {children}
    </LanguageContext.Provider>
  );
};

export const useLanguage = () => useContext(LanguageContext);
