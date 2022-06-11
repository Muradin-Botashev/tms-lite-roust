import { all, delay, put, takeEvery, select } from 'redux-saga/effects';
import { createSelector } from 'reselect';
import { downloader, postman, setAccessToken } from '../utils/postman';
import downloadFile from '../utils/downloadFile';
import { toast } from 'react-toastify';
import { showModal } from './modal';
import { filtersSelector, getListRequest } from './gridList';
import { currentLocationSelector } from './general';
import {ORDERS_GRID, SHIPPINGS_GRID} from '../constants/grids';

//*  TYPES  *//

const TEMPLATE_UPLOAD_REQUEST = 'TEMPLATE_UPLOAD_REQUEST';
const TEMPLATE_UPLOAD_SUCCESS = 'TEMPLATE_UPLOAD_SUCCESS';
const TEMPLATE_UPLOAD_ERROR = 'TEMPLATE_UPLOAD_ERROR';

const DATA_LOADING_REQUEST = 'DATA_LOADING_REQUEST';
const DATA_LOADING_SUCCESS = 'DATA_LOADING_SUCCESS';
const DATA_LOADING_ERROR = 'DATA_LOADING_ERROR';

const GET_INSTRUCTION_REQUEST = 'GET_INSTRUCTION_REQUEST';
const GET_INSTRUCTION_SUCCESS = 'GET_INSTRUCTION_SUCCESS';
const GET_INSTRUCTION_ERROR = 'GET_INSTRUCTION_ERROR';

const CHANGE_ORDERS_LOADING = 'CHANGE_ORDERS_LOADING';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    progress: false,
    ordersLoading: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case TEMPLATE_UPLOAD_REQUEST:
        case DATA_LOADING_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case TEMPLATE_UPLOAD_SUCCESS:
        case TEMPLATE_UPLOAD_ERROR:
        case DATA_LOADING_SUCCESS:
        case DATA_LOADING_ERROR:
            return {
                ...state,
                progress: false,
            };
        case CHANGE_ORDERS_LOADING:
            return {
                ...state,
                ordersLoading: payload,
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

export const templateUploadRequest = payload => {
    return {
        type: TEMPLATE_UPLOAD_REQUEST,
        payload,
    };
};

export const dataLoadingRequest = payload => {
    return {
        type: DATA_LOADING_REQUEST,
        payload,
    };
};

export const getInstructionRequest = payload => {
    return {
        type: GET_INSTRUCTION_REQUEST,
        payload
    }
};

//*  SELECTORS *//
const stateSelector = state => state.loadingData;
export const progressLoadingDataSelector = createSelector(
    stateSelector,
    state => state.progress,
);

export const ordersLoadingSelector = createSelector(stateSelector, state => state.ordersLoading);

//*  SAGA  *//

function* templateUploadSaga({ payload }) {
    try {
        const { typeApi } = payload;
        const res = yield downloader.get(`/import/${typeApi}/excelTemplate`, {
            responseType: 'blob',
        });
        downloadFile(res);
        yield put({ type: TEMPLATE_UPLOAD_SUCCESS });
    } catch (e) {
        yield put({
            type: TEMPLATE_UPLOAD_ERROR,
        });
    }
}

function* dataLoadingSaga({ payload }) {
    try {
        const { form, typeApi, callBackFunc } = payload;
        yield put({
            type: CHANGE_ORDERS_LOADING,
            payload: typeApi === 'orders',
        });
        const result = yield postman.post(`/import/${typeApi}/importFromExcel`, form, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });

        if (!result.isError) {
            yield put({
                type: DATA_LOADING_SUCCESS,
            });

            const currentLocation = yield select(currentLocationSelector);

            if (currentLocation.includes(SHIPPINGS_GRID) || currentLocation.includes(ORDERS_GRID)) {
                const filters = yield select(filtersSelector);
                yield put(getListRequest({ ...filters, notLoader: false }));
            }
        }



        if (result.isError && !result.entries.length) {
            toast.error(result.message);
        } else if (!result.isError && !result.entries.length && result.message) {
            toast.info(result.message);
        } else {
            yield put(showModal(result));
        }
    } catch (e) {
        yield put({
            type: DATA_LOADING_ERROR,
        });
    } finally {
        yield put({
            type: CHANGE_ORDERS_LOADING,
            payload: false,
        })
    }
}

function* getInstructionSaga({ payload }) {
    try {
        const { fileName } = payload;

        const result = yield downloader.get(`/static/${fileName}`, { responseType: 'blob' });
        const { data } = result;
        const link = document.createElement('a');
        link.href = URL.createObjectURL(new Blob([data], { type: 'application/pdf' }));
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();

        yield put({
            type: GET_INSTRUCTION_SUCCESS
        });
    } catch (e) {
        yield put({
            type: GET_INSTRUCTION_ERROR
        })
    }
}

export function* saga() {
    yield all([
        takeEvery(TEMPLATE_UPLOAD_REQUEST, templateUploadSaga),
        takeEvery(DATA_LOADING_REQUEST, dataLoadingSaga),
        takeEvery(GET_INSTRUCTION_REQUEST, getInstructionSaga),
    ]);
}
