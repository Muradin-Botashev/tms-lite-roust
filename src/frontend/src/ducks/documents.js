import { createSelector } from 'reselect';
import { downloader, postman } from '../utils/postman';
import { all, put, takeEvery } from 'redux-saga/effects';

//*  TYPES  *//

const UPLOAD_FILE_REQUEST = 'UPLOAD_FILE_REQUEST';
const UPLOAD_FILE_SUCCESS = 'UPLOAD_FILE_SUCCESS';
const UPLOAD_FILE_ERROR = 'UPLOAD_FILE_ERROR';

const GET_DOCUMENT_TYPES_REQUEST = 'GET_DOCUMENT_TYPES_REQUEST';
const GET_DOCUMENT_TYPES_SUCCESS = 'GET_DOCUMENT_TYPES_SUCCESS';
const GET_DOCUMENT_TYPES_ERROR = 'GET_DOCUMENT_TYPES_ERROR';

const GET_DOCUMENTS_REQUEST = 'GET_DOCUMENTS_REQUEST';
const GET_DOCUMENTS_SUCCESS = 'GET_DOCUMENTS_SUCCESS';
const GET_DOCUMENTS_ERROR = 'GET_DOCUMENTS_ERROR';

const ADD_DOCUMENT_REQUEST = 'ADD_DOCUMENT_REQUEST';
const ADD_DOCUMENT_SUCCESS = 'ADD_DOCUMENT_SUCCESS';
const ADD_DOCUMENT_ERROR = 'ADD_DOCUMENT_ERROR';

const EDIT_DOCUMENT_REQUEST = 'EDIT_DOCUMENT_REQUEST';
const EDIT_DOCUMENT_SUCCESS = 'EDIT_DOCUMENT_SUCCESS';
const EDIT_DOCUMENT_ERROR = 'EDIT_DOCUMENT_ERROR';

const DELETE_DOCUMENT_REQUEST = 'DELETE_DOCUMENT_REQUEST';
const DELETE_DOCUMENT_SUCCESS = 'DELETE_DOCUMENT_SUCCESS';
const DELETE_DOCUMENT_ERROR = 'DELETE_DOCUMENT_ERROR';

const DOWNLOAD_DOCUMENT_REQUEST = 'DOWNLOAD_DOCUMENT_REQUEST';
const DOWNLOAD_DOCUMENT_SUCCESS = 'DOWNLOAD_DOCUMENT_SUCCESS';
const DOWNLOAD_DOCUMENT_ERROR = 'DOWNLOAD_DOCUMENT_ERROR';

const CLEAR_DOCUMENTS = 'CLEAR_DOCUMENTS';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    progress: false,
    uploadProgress: false,
    addProgress: false,
    documentTypes: [],
    documents: [],
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_DOCUMENTS_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_DOCUMENTS_SUCCESS:
            return {
                ...state,
                documents: payload,
                progress: false,
            };
        case GET_DOCUMENTS_ERROR:
            return {
                ...state,
                progress: false,
                documents: [],
            };
        case UPLOAD_FILE_REQUEST:
            return {
                ...state,
                uploadProgress: true,
            };
        case UPLOAD_FILE_ERROR:
        case UPLOAD_FILE_SUCCESS:
            return {
                ...state,
                uploadProgress: false,
            };
        case GET_DOCUMENT_TYPES_SUCCESS:
            return {
                ...state,
                documentTypes: payload,
            };
        case ADD_DOCUMENT_REQUEST:
            return {
                ...state,
                addProgress: true,
            };
        case ADD_DOCUMENT_SUCCESS:
        case ADD_DOCUMENT_ERROR:
            return {
                ...state,
                addProgress: false,
            };
        case CLEAR_DOCUMENTS:
            return {
                ...state,
                documents: [],
            };
        case DEFAULT_STATE:
            return {
                ...initial
            };
        default:
            return state;
    }
};

//*  ACTION CREATORS  *//

export const uploadFileRequest = payload => {
    return {
        type: UPLOAD_FILE_REQUEST,
        payload,
    };
};

export const getDocumentTypesRequest = payload => {
    return {
        type: GET_DOCUMENT_TYPES_REQUEST,
        payload,
    };
};

export const addDocumentRequest = payload => {
    return {
        type: ADD_DOCUMENT_REQUEST,
        payload,
    };
};

export const getDocumentsRequest = payload => {
    return {
        type: GET_DOCUMENTS_REQUEST,
        payload,
    };
};

export const editDocumentRequest = payload => {
    return {
        type: EDIT_DOCUMENT_REQUEST,
        payload,
    };
};

export const deleteDocumentRequest = payload => {
    return {
        type: DELETE_DOCUMENT_REQUEST,
        payload,
    };
};

export const downloadDocumentRequest = payload => {
    return {
        type: DOWNLOAD_DOCUMENT_REQUEST,
        payload,
    };
};

export const clearDocuments = () => {
    return {
        type: CLEAR_DOCUMENTS,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.documents;

export const uploadProgressSelector = createSelector(stateSelector, state => state.uploadProgress);

export const documentTypesSelector = createSelector(
    stateSelector,
    state =>
        state.documentTypes &&
        state.documentTypes.map(x => ({
            key: x.id,
            value: x.id,
            text: x.name,
        })),
);

export const documentsSelector = createSelector(stateSelector, state => state.documents);
export const progressSelector = createSelector(stateSelector, state => state.progress);

//*  SAGA  *//

function* uploadFileSaga({ payload }) {
    try {
        const { form, fileName, callbackSuccess, isBase64 } = payload;
        const result = yield postman.post(`/files/${isBase64 ? 'base64' : 'upload'}`, form, {
            headers: {
                'Content-Type': isBase64 ? 'application/json-patch+json' : 'multipart/form-data',
            },
        });

        yield put({
            type: UPLOAD_FILE_SUCCESS,
            payload: {
                id: result.id,
                fileName,
            },
        });

        callbackSuccess(result.id);
    } catch (e) {
        yield put({
            type: UPLOAD_FILE_ERROR,
            payload: e,
        });
    }
}

function* getDocumentTypesSaga({ payload }) {
    try {
        const result = yield postman.post(`/documentTypes/search`, {filter: {}});

        yield put({
            type: GET_DOCUMENT_TYPES_SUCCESS,
            payload: result.items,
        });
    } catch (e) {
        yield put({
            type: GET_DOCUMENT_TYPES_ERROR,
            payload: e,
        });
    }
}

function* addDocumentSaga({ payload }) {
    try {
        const { gridName, cardId, document, callbackSuccess } = payload;
        const result = yield postman.post(`/${gridName}/${cardId}/documents`, document);

        yield put({
            type: ADD_DOCUMENT_SUCCESS,
        });

        callbackSuccess();
    } catch (e) {
        yield put({
            type: ADD_DOCUMENT_ERROR,
            payload: e,
        });
    }
}

function* getDocumentsSaga({ payload }) {
    try {
        const { gridName, cardId } = payload;
        const result = yield postman.get(`/${gridName}/${cardId}/documents`);

        yield put({
            type: GET_DOCUMENTS_SUCCESS,
            payload: result,
        });
    } catch (e) {
        yield put({
            type: GET_DOCUMENTS_ERROR,
            payload: e,
        });
    }
}

function* editDocumentSaga({ payload }) {
    try {
        const { gridName, cardId, id, document, callbackSuccess } = payload;
        const result = yield postman.put(`/${gridName}/${cardId}/documents/${id}`, document);

        yield put({
            type: EDIT_DOCUMENT_SUCCESS,
        });

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: EDIT_DOCUMENT_ERROR,
            payload: e,
        });
    }
}

function* deleteDocumentSaga({ payload }) {
    try {
        const { gridName, cardId, id, callbackSuccess } = payload;
        const result = yield postman.delete(`/${gridName}/${cardId}/documents/${id}`);

        yield put({
            type: DELETE_DOCUMENT_SUCCESS,
        });
        callbackSuccess();
    } catch (e) {
        yield put({
            type: DELETE_DOCUMENT_ERROR,
            payload: e,
        });
    }
}

function* downloadDocumentSaga({ payload }) {
    try {
        const { id } = payload;
        const res = yield downloader.get(`/files/${id}/download`, { responseType: 'blob' });
        const { data } = res;
        let headerLine = res.headers['content-disposition'].split('; ')[2];
        // console.log('headerLine', headerLine.split('; ')[2]);
        /*  let startFileNameIndex = headerLine[2].indexOf("''");
          /!*let endFileNameIndex = headerLine[2].lastIndexOf('"');*!/*/
        let filename = decodeURI(headerLine.substring(17));
        const link = document.createElement('a');
        link.href = URL.createObjectURL(new Blob([data], { type: data.type }));
        link.setAttribute('download', filename);
        document.body.appendChild(link);
        link.click();

        yield put({
            type: DOWNLOAD_DOCUMENT_SUCCESS,
        });
    } catch (e) {
        yield put({
            type: DOWNLOAD_DOCUMENT_ERROR,
            payload: e,
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(UPLOAD_FILE_REQUEST, uploadFileSaga),
        takeEvery(GET_DOCUMENT_TYPES_REQUEST, getDocumentTypesSaga),
        takeEvery(ADD_DOCUMENT_REQUEST, addDocumentSaga),
        takeEvery(GET_DOCUMENTS_REQUEST, getDocumentsSaga),
        takeEvery(EDIT_DOCUMENT_REQUEST, editDocumentSaga),
        takeEvery(DELETE_DOCUMENT_REQUEST, deleteDocumentSaga),
        takeEvery(DOWNLOAD_DOCUMENT_REQUEST, downloadDocumentSaga),
    ]);
}
