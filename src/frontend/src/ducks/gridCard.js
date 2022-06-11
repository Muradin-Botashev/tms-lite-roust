import { createSelector } from 'reselect';
import { postman } from '../utils/postman';
import { all, call, fork, put, select, takeEvery } from 'redux-saga/effects';
import { toast } from 'react-toastify';
import { roleIdSelector } from './profile';
import { fieldsSettingSelector, getFieldsSettingSaga } from './fieldsSetting';
import { SETTINGS_TYPE_HIDE } from '../constants/formTypes';
import { errorMapping } from '../utils/errorMapping';
import {BOOLEAN_TYPE, CHECKBOX_TYPE, CONSTRUCTOR_TYPE, RETURN_COST_TYPE, SOLD_TO_TYPE} from "../constants/columnTypes";

//*  TYPES  *//

const OPEN_GRID_CARD_REQUEST = 'OPEN_GRID_CARD_REQUEST';
const OPEN_GRID_CARD_SUCCESS = 'OPEN_GRID_CARD_SUCCESS';
const OPEN_GRID_CARD_ERROR = 'OPEN_GRID_CARD_ERROR';

const CREATE_DRAFT_REQUEST = 'CREATE_DRAFT_REQUEST';
const CREATE_DRAFT_SUCCESS = 'CREATE_DRAFT_SUCCESS';
const CREATE_DRAFT_ERROR = 'CREATE_DRAFT_ERROR';

const GET_CARD_CONFIG_REQUEST = 'GET_CARD_CONFIG_REQUEST';
const GET_CARD_CONFIG_SUCCESS = 'GET_CARD_CONFIG_SUCCESS';
const GET_CARD_CONFIG_ERROR = 'GET_CARD_CONFIG_ERROR';

const GET_GRID_CARD_REQUEST = 'GET_GRID_CARD_REQUEST';
const GET_GRID_CARD_SUCCESS = 'GET_GRID_CARD_SUCCESS';
const GET_GRID_CARD_ERROR = 'GET_GRID_CARD_ERROR';

const EDI_GRID_CARD_REQUEST = 'EDI_GRID_CARD_REQUEST';
const EDIT_GRID_CARD_SUCCESS = 'EDIT_GRID_CARD_SUCCESS';
const EDIT_GRID_CARD_ERROR = 'EDIT_GRID_CARD_ERROR';

const IS_UNIQUE_NUMBER_REQUEST = 'IS_UNIQUE_NUMBER_REQUEST';
const IS_UNIQUE_NUMBER_SUCCESS = 'IS_UNIQUE_NUMBER_SUCCESS';
const IS_UNIQUE_NUMBER_ERROR = 'IS_UNIQUE_NUMBER_ERROR';

const CREATE_GRID_CARD_REQUEST = 'CREATE_GRID_CARD_REQUEST';
const CREATE_GRID_CARD_SUCCESS = 'CREATE_GRID_CARD_SUCCESS';
const CREATE_GRID_CARD_ERROR = 'CREATE_GRID_CARD_ERROR';


const CLEAR_GRID_CARD = 'CLEAR_GRID_CARD';

const ADD_ERROR = 'ADD_ERROR';
const CLEAR_ERROR = 'CLEAR_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    config: {},
    data: {},
    error: [],
    progress: false,
    editProgress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_GRID_CARD_REQUEST:
        case CREATE_DRAFT_REQUEST:
        case GET_CARD_CONFIG_REQUEST:
            return {
                ...state,
                progress: true,
            };
        case GET_GRID_CARD_SUCCESS:
            return {
                ...state,
                progress: false,
                data: payload,
            };
        case CREATE_DRAFT_SUCCESS:
            return {
                ...state,
                progress: false,
                data: { id: payload.id },
            };
        case GET_CARD_CONFIG_SUCCESS:
            return {
                ...state,
                progress: false,
                config: payload,
            };
        case GET_GRID_CARD_ERROR:
        case CREATE_DRAFT_ERROR:
        case GET_CARD_CONFIG_ERROR:
            return {
                ...state,
                data: {},
                progress: false,
            };
        case OPEN_GRID_CARD_REQUEST:
            return {
                ...state,
                progress: true,
                data: {
                    id: payload.id,
                },
            };
        case OPEN_GRID_CARD_SUCCESS:
        case OPEN_GRID_CARD_ERROR:
            return {
                ...state,
                progress: false,
            };
        case EDI_GRID_CARD_REQUEST:
        case CREATE_GRID_CARD_REQUEST:
            return {
                ...state,
                editProgress: true,
            };
        case EDIT_GRID_CARD_SUCCESS:
        case CREATE_GRID_CARD_SUCCESS:
            return {
                ...state,
                error: [],
                editProgress: false,
            };
        case EDIT_GRID_CARD_ERROR:
        case CREATE_GRID_CARD_ERROR:
            return {
                ...state,
                error: payload,
                editProgress: false,
            };
        case IS_UNIQUE_NUMBER_SUCCESS:
            return {
                ...state,
                error: [
                    ...state.error,
                    {
                        name: payload.fieldName,
                        message: payload.errorText,
                    },
                ],
            };
        case CLEAR_GRID_CARD:
            return {
                ...state,
                error: [],
                data: {},
            };
        case CLEAR_ERROR:
            return {
                ...state,
                error: state.error && state.error.filter(item => item.name !== payload),
            };
        case ADD_ERROR:
            return {
                ...state,
                error: [...state.error, payload],
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

export const openGridCardRequest = payload => {
    return {
        type: OPEN_GRID_CARD_REQUEST,
        payload,
    };
};

export const createDraftRequest = payload => {
    return {
        type: CREATE_DRAFT_REQUEST,
        payload,
    };
};

export const createGridCardRequest = payload => {
    return {
        type: CREATE_GRID_CARD_REQUEST,
        payload,
    }
};

export const getCardRequest = payload => {
    return {
        type: GET_GRID_CARD_REQUEST,
        payload,
    };
};

export const getCardConfigRequest = payload => {
    return {
        type: GET_CARD_CONFIG_REQUEST,
        payload,
    };
};

export const editCardRequest = payload => {
    return {
        type: EDI_GRID_CARD_REQUEST,
        payload,
    };
};

export const isUniqueNumberRequest = payload => {
    return {
        type: IS_UNIQUE_NUMBER_REQUEST,
        payload,
    };
};

export const clearGridCard = () => {
    return {
        type: CLEAR_GRID_CARD,
    };
};

export const clearError = payload => {
    return {
        type: CLEAR_ERROR,
        payload,
    };
};

export const addError = payload => {
    return {
        type: ADD_ERROR,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.gridCard;

const gridName = (state, name) => name;

const idSelector = createSelector(stateSelector, state => state.data.id);

export const progressSelector = createSelector(stateSelector, state => state.progress);
export const editProgressSelector = createSelector(stateSelector, state => state.editProgress);

export const cardSelector = createSelector(stateSelector, state => state.data);

export const settingsFormSelector = createSelector(
    [fieldsSettingSelector, (state, status) => status],
    (state, status) => {
        let settings = {};
        const { base = [] } = state;
        base.forEach(item => {
            settings = {
                ...settings,
                [item.fieldName]: item.isHidden ? SETTINGS_TYPE_HIDE : item.accessTypes[status],
            };
        });

        return settings;
    },
);

export const settingsExtSelector = createSelector(
    [fieldsSettingSelector, (state, status) => status],
    (state, status) => {
        let settings = {};
        const { ext = [] } = state;
        ext.forEach(item => {
            settings = {
                ...settings,
                [item.fieldName]: item.isHidden ? SETTINGS_TYPE_HIDE : item.accessTypes[status],
            };
        });
        return settings;
    },
);

export const errorSelector = createSelector(stateSelector, state => errorMapping(state.error));

export const createDefaultValueSelector = createSelector(stateSelector, state => state.config.defaultValues);
export const createConfigSelector = createSelector(stateSelector, state => {
    let config = {};

    state.config.config && state.config.config.columns.forEach(column => {
        config = {
            ...config,
            [column.name]: {
                ...column,
            }
        };
    });

    return config;
});

//*  SAGA  *//

function* openGridCardSaga({ payload }) {
    try {
        const { name, id: idRow, callbackSuccess } = payload;

        if (!idRow) {
            yield call(createDraftSaga, { payload: { name } });
        }

        const id = yield select(idSelector);

        // yield call(getCardConfigSaga, { payload: { name, id } });

        yield call(getCardSaga, { payload: { name, id } });

        const card = yield select(cardSelector);

        callbackSuccess(card);
    } catch (error) {
        yield put({
            type: OPEN_GRID_CARD_ERROR,
            payload: error,
        });
    }
}

function* createDraftSaga({ payload }) {
    try {
        const { name } = payload;
        const result = yield postman.post(`/${name}/saveOrCreate`, {});

        yield put({
            type: CREATE_DRAFT_SUCCESS,
            payload: result,
        });
    } catch (error) {
        yield put({
            type: CREATE_DRAFT_ERROR,
            payload: error,
        });
    }
}

function* editCardSaga({ payload }) {
    try {
        const { name, params, callbackSuccess } = payload;
        const result = yield postman.post(`/${name}/saveOrCreate`, params);

        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: EDIT_GRID_CARD_ERROR,
                payload: result.errors,
            });
        } else {
            yield put({
                type: EDIT_GRID_CARD_SUCCESS,
                payload: result,
            });

            toast.info(result.message);

            callbackSuccess && callbackSuccess();
        }
    } catch (error) {
        yield put({
            type: EDIT_GRID_CARD_ERROR,
        });
    }
}

function* createCardSaga({ payload }) {
    try {
        const { name, params, callbackSuccess } = payload;
        const result = yield postman.post(`/${name}/create`, params);

        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: CREATE_GRID_CARD_ERROR,
                payload: result.errors,
            });
        } else {
            yield put({
                type: CREATE_GRID_CARD_SUCCESS,
                payload: result,
            });

            toast.info(result.message);

            callbackSuccess && callbackSuccess();
        }
    } catch (error) {
        yield put({
            type: CREATE_GRID_CARD_ERROR,
        });
    }
}

function* getCardConfigSaga({ payload }) {
    try {
        const { name, id } = payload;
        const config = yield postman.get(`${name}/create/appConfiguration`);
        const defaultValues = yield postman.get(`${name}/create/defaults`);

        yield put({
            type: GET_CARD_CONFIG_SUCCESS,
            payload: {
                config,
                defaultValues
            },
        });
    } catch (error) {
        yield put({
            type: GET_CARD_CONFIG_ERROR,
            payload: error,
        });
    }
}

function* getCardSaga({ payload }) {
    try {
        const { name, id, callbackSuccess } = payload;
        const result = yield postman.get(`${name}/getById/${id}`);
        yield put({ type: GET_GRID_CARD_SUCCESS, payload: result });
        callbackSuccess && callbackSuccess(result);
    } catch (error) {
        yield put({ type: GET_GRID_CARD_ERROR });
    }
}

function* isUniqueNumberSaga({ payload }) {
    try {
        const { number, fieldName, errorText, callbackSuccess } = payload;
        const result = yield postman.post('/orders/findNumber', { number, isPartial: false });

        if (result.length && result[0].name) {
            yield put({
                type: IS_UNIQUE_NUMBER_SUCCESS,
                payload: {
                    fieldName,
                    errorText,
                },
            });
        } else {
            callbackSuccess && callbackSuccess();
        }
    } catch (e) {
        yield put({
            type: IS_UNIQUE_NUMBER_ERROR,
            e
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(OPEN_GRID_CARD_REQUEST, openGridCardSaga),
        takeEvery(CREATE_DRAFT_REQUEST, createDraftSaga),
        takeEvery(GET_CARD_CONFIG_REQUEST, getCardConfigSaga),
        takeEvery(GET_GRID_CARD_REQUEST, getCardSaga),
        takeEvery(EDI_GRID_CARD_REQUEST, editCardSaga),
        takeEvery(CREATE_GRID_CARD_REQUEST, createCardSaga),
        takeEvery(IS_UNIQUE_NUMBER_REQUEST, isUniqueNumberSaga),
    ]);
}
