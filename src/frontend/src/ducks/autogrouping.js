import { createSelector } from 'reselect';
import { downloader, postman } from '../utils/postman';
import { all, delay, put, takeEvery, select, call } from 'redux-saga/effects';
import { ORDERS_GRID } from '../constants/grids';
import { columnsGridSelector } from './gridList';
import downloadFile from '../utils/downloadFile';
import {
    BIG_TEXT_TYPE,
    CUSTOM_SELECT_TYPE,
    CUSTOM_STATE_TYPE,
    DATE_TYPE,
    ENUM_TYPE,
    INTEGER_TYPE,
    LINK_TYPE,
    NUMBER_TYPE,
    SELECT_TYPE,
    STATE_TYPE,
    TEXT_TYPE,
} from '../constants/columnTypes';
import { isRange } from '../utils/dateTimeFormater';
import { toast } from 'react-toastify';
import { showModal } from './modal';

const TYPE_API = 'autogrouping';

//*  TYPES  *//

const GET_GROUP_FIELDS_REQUEST = 'GET_GROUP_FIELDS_REQUEST';
const GET_GROUP_FIELDS_SUCCESS = 'GET_GROUP_FIELDS_SUCCESS';
const GET_GROUP_FIELDS_ERROR = 'GET_GROUP_FIELDS_ERROR';

const PREVIEW_AUTO_GROUPING_REQUEST = 'PREVIEW_AUTO_GROUPING_REQUEST';
const PREVIEW_AUTO_GROUPING_SUCCESS = 'PREVIEW_AUTO_GROUPING_SUCCESS';
const PREVIEW_AUTO_GROUPING_ERROR = 'PREVIEW_AUTO_GROUPING_ERROR';

const GET_PREVIEW_AUTO_GROUPING_REQUEST = 'GET_PREVIEW_AUTO_GROUPING_REQUEST';
const GET_PREVIEW_AUTO_GROUPING_SUCCESS = 'GET_PREVIEW_AUTO_GROUPING_SUCCESS';
const GET_PREVIEW_AUTO_GROUPING_ERROR = 'GET_PREVIEW_AUTO_GROUPING_ERROR';

const GET_EXT_PREVIEW_AUTO_GROUPING_REQUEST = 'GET_EXT_PREVIEW_AUTO_GROUPING_REQUEST';
const GET_EXT_PREVIEW_AUTO_GROUPING_SUCCESS = 'GET_EXT_PREVIEW_AUTO_GROUPING_SUCCESS';
const GET_EXT_PREVIEW_AUTO_GROUPING_ERROR = 'GET_EXT_PREVIEW_AUTO_GROUPING_ERROR';

const CLEAR_EXT_PREVIEW = 'CLEAR_EXT_PREVIEW';

const PREVIEW_EXPORT_TO_EXCEL_REQUEST = 'PREVIEW_EXPORT_TO_EXCEL_REQUEST';
const PREVIEW_EXPORT_TO_EXCEL_SUCCESS = 'PREVIEW_EXPORT_TO_EXCEL_SUCCESS';
const PREVIEW_EXPORT_TO_EXCEL_ERROR = 'PREVIEW_EXPORT_TO_EXCEL_ERROR';

const APPLY_AUTO_GROUPING_REQUEST = 'APPLY_AUTO_GROUPING_REQUEST';
const APPLY_AUTO_GROUPING_SUCCESS = 'APPLY_AUTO_GROUPING_SUCCESS';
const APPLY_AUTO_GROUPING_ERROR = 'APPLY_AUTO_GROUPING_ERROR';

const APPLY_AND_SEND_AUTO_GROUPING_REQUEST = 'APPLY_AND_SEND_AUTO_GROUPING_REQUEST';
const APPLY_AND_SEND_AUTO_GROUPING_SUCCESS = 'APPLY_AND_SEND_AUTO_GROUPING_SUCCESS';
const APPLY_AND_SEND_AUTO_GROUPING_ERROR = 'APPLY_AND_SEND_AUTO_GROUPING_ERROR';

const GET_SETTINGS_AUTO_GROUPING_REQUEST = 'GET_SETTINGS_AUTO_GROUPING_REQUEST';
const GET_SETTINGS_AUTO_GROUPING_SUCCESS = 'GET_SETTINGS_AUTO_GROUPING_SUCCESS';
const GET_SETTINGS_AUTO_GROUPING_ERROR = 'GET_SETTINGS_AUTO_GROUPING_ERROR';

const CHANGE_CARRIER_REQUEST = 'CHANGE_CARRIER_REQUEST';
const CHANGE_CARRIER_SUCCESS = 'CHANGE_CARRIER_SUCCESS';
const CHANGE_CARRIER_ERROR = 'CHANGE_CARRIER_ERROR';

const MOVE_ORDERS_REQUEST = 'MOVE_ORDERS_REQUEST';
const MOVE_ORDERS_SUCCESS = 'MOVE_ORDERS_SUCCESS';
const MOVE_ORDERS_ERROR = 'MOVE_ORDERS_ERROR';

const GET_CONFIG = 'GET_CONFIG';
const GET_EXT_CONFIG = 'GET_EXT_CONFIG';

const CHANGE_IS_GROUPING = 'CHANGE_IS_GROUPING';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    groupFields: [],
    getGroupFieldsProgress: false,
    runId: null,
    preview: {},
    extPreview: {},
    extPreview_backup: {},
    previewProgress: false,
    exportProgress: false,
    getProgress: false,
    extProgress: false,
    result: {},
    config: {},
    extConfig: {},
    applyProgress: false,
    isGrouping: false,
    settings: {},
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_GROUP_FIELDS_REQUEST:
            return {
                ...state,
                getGroupFieldsProgress: true,
            };
        case GET_GROUP_FIELDS_SUCCESS:
            return {
                ...state,
                groupFields: payload,
                getGroupFieldsProgress: false,
            };
        case GET_GROUP_FIELDS_ERROR:
            return {
                ...state,
                groupFields: [],
                getGroupFieldsProgress: false,
            };
        case PREVIEW_AUTO_GROUPING_REQUEST:
            return {
                ...state,
                runId: null,
                isGrouping: false,
                previewProgress: true,
            };
        case GET_PREVIEW_AUTO_GROUPING_REQUEST:
            return {
                ...state,
                getProgress: true,
            };
        case PREVIEW_AUTO_GROUPING_SUCCESS:
            return {
                ...state,
                runId: payload,
                previewProgress: false,
            };
        case GET_PREVIEW_AUTO_GROUPING_SUCCESS:
            return {
                ...state,
                preview: payload,
                isGrouping: true,
                getProgress: false,
            };
        case GET_PREVIEW_AUTO_GROUPING_ERROR:
            return {
                ...state,
                preview: [],
                getProgress: false,
            };
        case PREVIEW_AUTO_GROUPING_ERROR:
            return {
                ...state,
                preview: null,
                isGrouping: false,
                previewProgress: false,
            };
        case GET_EXT_PREVIEW_AUTO_GROUPING_REQUEST:
            return {
                ...state,
                extProgress: true,
            };
        case GET_EXT_PREVIEW_AUTO_GROUPING_SUCCESS:
            return {
                ...state,
                extPreview: {
                    ...state.extPreview,
                    ...payload,
                },
                extProgress: false,
            };
        case GET_EXT_PREVIEW_AUTO_GROUPING_ERROR:
            return {
                ...state,
                extProgress: false,
            };
        case PREVIEW_EXPORT_TO_EXCEL_REQUEST:
            return {
                ...state,
                exportProgress: true,
            };
        case PREVIEW_EXPORT_TO_EXCEL_SUCCESS:
        case PREVIEW_EXPORT_TO_EXCEL_ERROR:
            return {
                ...state,
                exportProgress: false,
            };
        case APPLY_AUTO_GROUPING_REQUEST:
            return {
                ...state,
                applyProgress: true,
            };
        case APPLY_AUTO_GROUPING_SUCCESS:
            return {
                ...state,
                applyProgress: false,
                result: payload,
            };
        case APPLY_AUTO_GROUPING_ERROR:
            return {
                ...state,
                applyProgress: false,
                result: {},
            };
        case APPLY_AND_SEND_AUTO_GROUPING_REQUEST:
            return {
                ...state,
                applyProgress: true,
            };
        case APPLY_AND_SEND_AUTO_GROUPING_SUCCESS:
            return {
                ...state,
                applyProgress: false,
                result: payload,
            };
        case APPLY_AND_SEND_AUTO_GROUPING_ERROR:
            return {
                ...state,
                applyProgress: false,
                result: {},
            };
        case GET_SETTINGS_AUTO_GROUPING_SUCCESS: {
            return {
                ...state,
                settings: payload,
            };
        }
        case CHANGE_IS_GROUPING:
            return {
                ...state,
                isGrouping: payload,
                preview: {},
                result: {},
                runId: null,
            };
        case GET_CONFIG:
            return {
                ...state,
                config: payload,
            };
        case GET_EXT_CONFIG:
            return {
                ...state,
                extConfig: payload,
            };
        case MOVE_ORDERS_REQUEST:
            const orders = state.extPreview[payload.oldShipping].filter(i => payload.params.orderIds.includes(i.id));
            return {
                ...state,
                extPreview_backup: {...state.extPreview},
                extPreview: {
                    ...state.extPreview,
                    [payload.oldShipping]: state.extPreview[payload.oldShipping].filter(i => !payload.params.orderIds.includes(i.id)),
                    [payload.params.newShippingId]: [...state.extPreview[payload.params.newShippingId], ...orders],
                }
            };
        case MOVE_ORDERS_SUCCESS:
            return {
                ...state,
                extPreview_backup: {}
            };
        case MOVE_ORDERS_ERROR:
            return {
                ...state,
                extPreview: {...state.extPreview_backup},
                extPreview_backup: {}
            };
        case CLEAR_EXT_PREVIEW:
            const newObj = {...state.extPreview};
            delete newObj[payload];
            return {
                ...state,
                extPreview: {...newObj},
            };
        case DEFAULT_STATE:
            return {
                ...initial,
            };
        default:
            return state;
    }
};

//*  ACTION CREATORS  *//
export const getGroupFieldsRequest = payload => {
    return {
        type: GET_GROUP_FIELDS_REQUEST,
        payload,
    };
};

export const previewAutoGroupingRequest = payload => {
    return {
        type: PREVIEW_AUTO_GROUPING_REQUEST,
        payload,
    };
};

export const previewExportToExcelRequest = payload => {
    return {
        type: PREVIEW_EXPORT_TO_EXCEL_REQUEST,
        payload,
    };
};

export const applyAutoGroupingRequest = payload => {
    return {
        type: APPLY_AUTO_GROUPING_REQUEST,
        payload,
    };
};

export const applyAndSendAutoGroupingRequest = payload => {
    return {
        type: APPLY_AND_SEND_AUTO_GROUPING_REQUEST,
        payload,
    };
};

export const changeIsGrouping = payload => {
    return {
        type: CHANGE_IS_GROUPING,
        payload,
    };
};

export const getPreviewRequest = payload => {
    return {
        type: GET_PREVIEW_AUTO_GROUPING_REQUEST,
        payload,
    };
};

export const getExtPreviewRequest = payload => {
    return {
        type: GET_EXT_PREVIEW_AUTO_GROUPING_REQUEST,
        payload,
    };
};

export const clearExtPreview = payload => {
    return {
        type: CLEAR_EXT_PREVIEW,
        payload
    };
};

export const changeCarrierRequest = payload => {
    return {
        type: CHANGE_CARRIER_REQUEST,
        payload
    }
};

export const moveOrdersRequest = payload => {
    return {
        type: MOVE_ORDERS_REQUEST,
        payload
    }
};

//*  SELECTORS *//
const stateSelector = state => state.autogrouping;

export const previewOrdersSelector = createSelector(
    stateSelector,
    state => state.preview && state.preview.items || [],
);
export const extPreviewOrdersSelector = createSelector(
    [stateSelector, (state, parentId) => parentId],
    (state, parentId) => state.extPreview && state.extPreview[parentId] || [],
);
export const infoSelector = createSelector(
    stateSelector,
    state => state.preview && state.preview.info,
);
export const previewColumnsSelector = createSelector(
    stateSelector,
    state => state.config.columns || [],
);
export const previewExtColumnsSelector = createSelector(
    stateSelector,
    state => state.extConfig.columns || [],
);
export const isGroupingSelector = createSelector(
    stateSelector,
    state => state && state.isGrouping,
);

export const resultSelector = createSelector(
    stateSelector,
    state => state.result.entries,
);
export const resultMessageSelector = createSelector(
    stateSelector,
    state => state.result.message,
);

export const previewProgressSelector = createSelector(
    stateSelector,
    state => state.previewProgress,
);
export const extPreviewProgressSelector = createSelector(
    stateSelector,
    state => state.extProgress,
);
export const exportProgressSelector = createSelector(
    stateSelector,
    state => state.exportProgress,
);
export const applyProgressSelector = createSelector(
    stateSelector,
    state => state.applyProgress,
);

export const runIdSelector = createSelector(
    stateSelector,
    state => state.runId,
);

export const settingsSelector = createSelector(
    stateSelector,
    state => state.settings,
);

//*  SAGA  *//

function* getGroupFieldsSaga({ payload }) {
    try {
        const { callBackSuccess } = payload;
        const result = yield postman.get(`/${TYPE_API}/groupFields`);

        yield put({
            type: GET_GROUP_FIELDS_SUCCESS,
            payload: result,
        });

        callBackSuccess && callBackSuccess(result);
    } catch (e) {
        yield put({
            type: GET_GROUP_FIELDS_ERROR,
        });
    }
}

function* getConfig() {
    try {
        const result = yield postman.get(`/${TYPE_API}/previewConfiguration`);

        yield put({
            type: GET_CONFIG,
            payload: result,
        });
    } catch (e) {
        console.log('error');
    }
}

function* getExtConfig() {
    try {
        const result = yield postman.get(`/autogroupingOrders/previewConfiguration`);

        yield put({
            type: GET_EXT_CONFIG,
            payload: result,
        });
    } catch (e) {
        console.log('error');
    }
}

function* previewAutoGroupSaga({ payload }) {
    try {
        const { params: propsParams, callBackSuccess, runId } = payload;

        let params = {...propsParams};

        if (!params.autogroupingTypes) {
            const autogroupingTypes = yield postman.get(`/${TYPE_API}/autogroupingTypes`);

            yield put({
                type: GET_SETTINGS_AUTO_GROUPING_SUCCESS,
                payload: autogroupingTypes,
            });

            params = {
                ...params,
                autogroupingTypes: autogroupingTypes.selected,
            }
        }

        if (!runId) {
            const result = yield postman.post(`/${TYPE_API}/run`, params);

            yield put({
                type: PREVIEW_AUTO_GROUPING_SUCCESS,
                payload: result.runId,
            });
        } else {
            yield put({
                type: PREVIEW_AUTO_GROUPING_SUCCESS,
                payload: runId,
            });
        }
        yield call(getConfig);
        yield call(getExtConfig);
        //callBackSuccess && callBackSuccess(result.orders);
    } catch (e) {
        yield put({
            type: PREVIEW_AUTO_GROUPING_ERROR,
        });
    }
}

function* getPreviewSaga({ payload }) {
    try {
        const { params, runId } = payload;
        const result = yield postman.post(`/${TYPE_API}/${runId}/search`, params);
        const info = yield postman.get(`/${TYPE_API}/${runId}/getSummary`);

        yield put({
            type: GET_PREVIEW_AUTO_GROUPING_SUCCESS,
            payload: {
                ...result,
                info,
            },
        });
    } catch (e) {
        yield put({
            type: GET_PREVIEW_AUTO_GROUPING_ERROR,
            payload: e,
        });
    }
}

function* getExtPreviewSaga({ payload }) {
    try {
        const { params, parentId } = payload;
        const runId = yield select(runIdSelector);
        const result = yield postman.post(
            `/autogroupingOrders/${runId}/${parentId}/search`,
            params,
        );

        yield put({
            type: GET_EXT_PREVIEW_AUTO_GROUPING_SUCCESS,
            payload: {
                [parentId]: result.items
            },
        });
    } catch (e) {
        yield put({
            type: GET_EXT_PREVIEW_AUTO_GROUPING_ERROR,
            payload: e,
        });
    }
}

function* previewExportToExcelSaga({ payload }) {
    try {
        const { runId, params } = payload;
        const result = yield downloader.post(`/${TYPE_API}/${runId}/exportToExcel`, params, {
            responseType: 'blob',
        });

        downloadFile(result);

        yield put({
            type: PREVIEW_EXPORT_TO_EXCEL_SUCCESS,
        });
    } catch (e) {
        yield put({
            type: PREVIEW_EXPORT_TO_EXCEL_ERROR,
        });
    }
}

function* applyAutoGroupingSaga({ payload }) {
    try {
        const { runId, params, callbackSuccess } = payload;

        const result = yield postman.post(`/${TYPE_API}/${runId}/apply`, params);

        if (!result.isError) {
            yield put({
                type: APPLY_AUTO_GROUPING_SUCCESS,
                payload: result,
            });

            callbackSuccess && callbackSuccess();
            toast.info(result.message);
        } else {
            yield put({
                type: APPLY_AUTO_GROUPING_ERROR,
            });
            toast.error(result.message);
        }
    } catch (e) {
        yield put({
            type: APPLY_AUTO_GROUPING_ERROR,
        });
    }
}

function* applyAndSendAutoGroupingSaga({ payload }) {
    try {
        const { runId, params, callbackSuccess } = payload;

        const result = yield postman.post(`/${TYPE_API}/${runId}/applyAndSend`, params);

        if (!result.isError) {
            yield put({
                type: APPLY_AND_SEND_AUTO_GROUPING_SUCCESS,
                payload: result,
            });

            callbackSuccess && callbackSuccess();
            // toast.info(result.message);
        } else {
            yield put({
                type: APPLY_AND_SEND_AUTO_GROUPING_ERROR,
            });
            //toast.error(result.message);
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
            type: APPLY_AND_SEND_AUTO_GROUPING_ERROR,
        });
    }
}

function* changeCarrierSaga({ payload}) {
    try {
        const {params, runId, callbackSuccess} = payload;

        const result = yield postman.post(`/${TYPE_API}/${runId}/changeCarrier`, params);

        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: CHANGE_CARRIER_ERROR
        })
    }
}

function* moveOrdersSaga({ payload }) {
    try {
        const {runId, params, callbackSuccess} = payload;
        const result = yield postman.post(`/${TYPE_API}/${runId}/moveOrders`, params);

        yield put({
            type: MOVE_ORDERS_SUCCESS,
        });

        callbackSuccess && callbackSuccess();

    } catch (e) {
        yield put({
            type: MOVE_ORDERS_ERROR
        })
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_GROUP_FIELDS_REQUEST, getGroupFieldsSaga),
        takeEvery(PREVIEW_AUTO_GROUPING_REQUEST, previewAutoGroupSaga),
        takeEvery(GET_PREVIEW_AUTO_GROUPING_REQUEST, getPreviewSaga),
        takeEvery(GET_EXT_PREVIEW_AUTO_GROUPING_REQUEST, getExtPreviewSaga),
        takeEvery(PREVIEW_EXPORT_TO_EXCEL_REQUEST, previewExportToExcelSaga),
        takeEvery(APPLY_AUTO_GROUPING_REQUEST, applyAutoGroupingSaga),
        takeEvery(APPLY_AND_SEND_AUTO_GROUPING_REQUEST, applyAndSendAutoGroupingSaga),
        takeEvery(CHANGE_CARRIER_REQUEST, changeCarrierSaga),
        takeEvery(MOVE_ORDERS_REQUEST, moveOrdersSaga),
    ]);
}
