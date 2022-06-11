import i18n from 'i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import { initReactI18next } from 'react-i18next';
import XHR from 'i18next-xhr-backend';

const language = window.navigator ? (window.navigator.language ||
    window.navigator.systemLanguage ||
    window.navigator.userLanguage) : "ru";


i18n.use(LanguageDetector)
    .use(XHR)
    .use(initReactI18next)
    .init({
        // we init with resources
        fallbackLng: 'en',
        lng: localStorage.getItem('i18nextLng') ? localStorage.getItem('i18nextLng') : language.includes('en') ? 'en' : 'ru',
        debug: false,

        // have a common namespace used around the full app
        ns: ['translations'],
        defaultNS: 'translations',
        backend: {
            loadPath: '/api/translation/GetForLangType/{{lng}}',
        },
        keySeparator: false, // we use content as keys
        react: {
            wait: true,
            defaultTransParent: 'div', // a valid react element - required before react 16
        },
    });

export default i18n;
