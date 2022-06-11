import { all, put, takeEvery, take, spawn, delay, cancelled, cancel, fork, select } from 'redux-saga/effects';
import { createSelector } from 'reselect';
import {downloader, postman} from '../utils/postman';
import downloadFile from "../utils/downloadFile";

const TYPE_API = 'reports';

//*  TYPES  *//

const GET_REPORT_REQUEST = 'GET_REPORT_REQUEST';
const GET_REPORT_SUCCESS = 'GET_REPORT_SUCCESS';
const GET_REPORT_ERROR = 'GET_REPORT_ERROR';

const REPORT_EXPORT_TO_EXCEL_REQUEST = 'REPORT_EXPORT_TO_EXCEL_REQUEST';
const REPORT_EXPORT_TO_EXCEL_SUCCESS = 'REPORT_EXPORT_TO_EXCEL_SUCCESS';
const REPORT_EXPORT_TO_EXCEL_ERROR = 'REPORT_EXPORT_TO_EXCEL_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    data: [],
    columns: [],
    config: [],
    progress: false,
    exportProgress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_REPORT_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_REPORT_SUCCESS:
            return {
                ...state,
                progress: false,
                ...payload,
            };
        case GET_REPORT_ERROR:
            return {
                ...state,
                progress: false,
            };
        case REPORT_EXPORT_TO_EXCEL_REQUEST:
            return {
                ...state,
                exportProgress: true
            };
        case REPORT_EXPORT_TO_EXCEL_SUCCESS:
        case REPORT_EXPORT_TO_EXCEL_ERROR:
            return {
                ...state,
                exportProgress: false
            };
        case DEFAULT_STATE:
            return {
                ...initial
            };
        default:
            return state;
    }
}

//*  ACTION CREATORS  *//

export const getReportRequest = (payload) => {
    return {
        type: GET_REPORT_REQUEST,
        payload
    }
};

export const reportExportToExcelRequest = payload => {
    return {
        type: REPORT_EXPORT_TO_EXCEL_REQUEST,
        payload
    }
};

//*  SELECTORS *//

const stateSelector = state => state.reports;
export const reportSelector = createSelector(stateSelector, state => state.data);

export const columnsSelector = createSelector(stateSelector, state => {
    return state.config.filter(item => state.columns.map(column => column.toLowerCase()).includes(item.name.toLowerCase()))
});

export const progressSelector = createSelector(stateSelector, state => state.progress);
export const exportProgressSelector = createSelector(stateSelector, state => state.exportProgress);

//*  SAGA  *//

function* getReportSaga({ payload }) {
    try {
        const {type, params} = payload;

        const config = yield postman.get(`/${TYPE_API}/${type}/reportConfiguration`);

        const result = yield postman.post(`/${TYPE_API}/${type}/get`, params);

        yield put({
            type: GET_REPORT_SUCCESS,
            payload: {
                ...result,
                config: config.columns
            }
        })
    } catch (e) {
        yield put({
            type: GET_REPORT_ERROR
        })
    }
}

function* reportExportToExcelSaga({ payload }) {
    try {
        const {type, params} = payload;

        const result = yield downloader.post(`/${TYPE_API}/${type}/export`, params, { responseType: 'blob' });

        downloadFile(result);

        yield put({type: REPORT_EXPORT_TO_EXCEL_SUCCESS})
    } catch (e) {
        yield put({
            type: REPORT_EXPORT_TO_EXCEL_ERROR
        })
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_REPORT_REQUEST, getReportSaga),
        takeEvery(REPORT_EXPORT_TO_EXCEL_REQUEST, reportExportToExcelSaga),
    ])
}
