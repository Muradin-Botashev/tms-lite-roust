import { all, put, takeEvery } from 'redux-saga/effects';
import { postman } from '../utils/postman';
import { createSelector } from 'reselect';
import users from '../mocks/users';
import { toast } from 'react-toastify';
import { errorMapping } from '../utils/errorMapping';

const TYPE_API = 'users';

//*  TYPES  *//

const GET_USERS_LIST_REQUEST = 'GET_USERS_LIST_REQUEST';
const GET_USERS_LIST_SUCCESS = 'GET_USERS_LIST_SUCCESS';
const GET_USERS_LIST_ERROR = 'GET_USERS_LIST_ERROR';

const GET_USER_CARD_REQUEST = 'GET_USER_CARD_REQUEST';
const GET_USER_CARD_SUCCESS = 'GET_USER_CARD_SUCCESS';
const GET_USER_CARD_ERROR = 'GET_USER_CARD_ERROR';

const CREATE_USER_REQUEST = 'CREATE_USER_REQUEST';
const CREATE_USER_SUCCESS = 'CREATE_USER_SUCCESS';
const CREATE_USER_ERROR = 'CREATE_USER_ERROR';

const TOGGLE_USER_ACTIVE_REQUEST = 'TOGGLE_USER_ACTIVE_REQUEST';
const TOGGLE_USER_ACTIVE_SUCCESS = 'TOGGLE_USER_ACTIVE_SUCCESS';
const TOGGLE_USER_ACTIVE_ERROR = 'TOGGLE_USER_ACTIVE_ERROR';

const GENERATE_KEY_OPEN_API_REQUEST = 'GENERATE_KEY_OPEN_API_REQUEST';
const GENERATE_KEY_OPEN_API_SUCCESS = 'GENERATE_KEY_OPEN_API_SUCCESS';
const GENERATE_KEY_OPEN_API_ERROR = 'GENERATE_KEY_OPEN_API_ERROR';

const CLEAR_USER_CARD = 'CLEAR_USER_CARD';
const CLEAR_ERROR = 'CLEAR_ERROR';

const DEFAULT_STATE = 'DEFAULT_STATE';

//*  INITIAL STATE  *//

const initial = {
    list: [],
    card: {},
    totalCount: 0,
    error: [],
    progress: false,
    loadingProgress: false,
};

//*  REDUCER  *//

export default (state = initial, { type, payload }) => {
    switch (type) {
        case GET_USERS_LIST_REQUEST:
        case GET_USER_CARD_REQUEST:
            return {
                ...state,
                loadingProgress: true,
                error: '',
            };
        case CREATE_USER_REQUEST:
            return {
                ...state,
                progress: true,
                error: '',
            };
        case GET_USERS_LIST_SUCCESS:
            return {
                ...state,
                loadingProgress: false,
                error: '',
                list: payload.isConcat ? [...state.list, ...payload.items] : payload.items,
                totalCount: payload.totalCount,
            };
        case GET_USER_CARD_SUCCESS:
            return {
                ...state,
                loadingProgress: false,
                card: payload,
            };
        case CREATE_USER_SUCCESS:
            return {
                ...state,
                progress: false,
                error: [],
            };
        case GET_USERS_LIST_ERROR:
        case GET_USER_CARD_ERROR:
            return {
                ...state,
                loadingProgress: false,
                error: payload,
            };
        case CREATE_USER_ERROR:
            return {
                ...state,
                progress: false,
                error: payload,
            };
        case CLEAR_USER_CARD:
            return {
                ...state,
                card: {},
                error: [],
            };
        case CLEAR_ERROR:
            return {
                ...state,
                error: state.error && state.error.filter(item => item.name !== payload),
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

export const getUsersRequest = payload => {
    return {
        type: GET_USERS_LIST_REQUEST,
        payload,
    };
};

export const getUserCardRequest = payload => {
    return {
        type: GET_USER_CARD_REQUEST,
        payload,
    };
};

export const createUserRequest = payload => {
    return {
        type: CREATE_USER_REQUEST,
        payload,
    };
};

export const clearUsersInfo = () => {
    return {
        type: DEFAULT_STATE,
    };
};

export const clearUserCard = () => {
    return {
        type: CLEAR_USER_CARD,
    };
};

export const toggleUserActiveRequest = payload => {
    return {
        type: TOGGLE_USER_ACTIVE_REQUEST,
        payload,
    };
};

export const clearError = payload => {
    return {
        type: CLEAR_ERROR,
        payload,
    };
};

export const generateKeyRequest = payload => {
    return {
        type: GENERATE_KEY_OPEN_API_REQUEST,
        payload,
    };
};

//*  SELECTORS *//

const stateSelector = state => state.users;
export const usersListSelector = createSelector(
    stateSelector,
    state => state.list,
);
export const progressSelector = createSelector(
    stateSelector,
    state => state.loadingProgress,
);
export const saveProgressSelector = createSelector(
    stateSelector,
    state => state.progress,
);
export const totalCountSelector = createSelector(
    stateSelector,
    state => state.totalCount,
);
export const userCardSelector = createSelector(
    stateSelector,
    state => state.card,
);
export const errorSelector = createSelector(
    stateSelector,
    state => errorMapping(state.error),
);

//*  SAGA  *//

function* getUsersListSaga({ payload }) {
    try {
        const { filter = {}, isConcat } = payload;
        const result = yield postman.post('/users/search', filter);

        yield put({
            type: GET_USERS_LIST_SUCCESS,
            payload: {
                ...result,
                isConcat,
            },
        });
    } catch (error) {
        yield put({
            type: GET_USERS_LIST_ERROR,
            payload: error,
        });
    }
}

function* getUserCardSaga({ payload }) {
    try {
        const result = yield postman.get(`/users/getById/${payload}`);

        yield put({
            type: GET_USER_CARD_SUCCESS,
            payload: result,
        });
    } catch (error) {
        yield put({
            type: GET_USER_CARD_ERROR,
            payload: error,
        });
    }
}

function* createUserSaga({ payload }) {
    try {
        const { params, callbackFunc } = payload;

        const result = yield postman.post('/users/saveOrCreate', params);

        if (result.isError) {
            toast.error(result.message);
            yield put({
                type: CREATE_USER_ERROR,
                payload: result.errors,
            });
        } else {
            yield put({
                type: CREATE_USER_SUCCESS,
            });
            callbackFunc && callbackFunc();
        }
    } catch (error) {
        yield put({
            type: CREATE_USER_ERROR,
        });
    }
}

function* toggleUserActiveSaga({ payload }) {
    try {
        const { id, active, callbackSuccess } = payload;
        const result = yield postman.post(`/users/setActive/${id}/${active}`);

        yield put({
            type: TOGGLE_USER_ACTIVE_SUCCESS,
        });
        callbackSuccess && callbackSuccess();
    } catch (e) {
        yield put({
            type: TOGGLE_USER_ACTIVE_ERROR,
            payload: e,
        });
    }
}

function* generateKeySaga({ payload }) {
    try {
        const { id, callbackSuccess } = payload;
        const result = yield postman.post(`users/newOpen/${id}`);

        callbackSuccess && callbackSuccess(result.token);
    } catch (e) {
        yield put({
            type: GENERATE_KEY_OPEN_API_ERROR,
        });
    }
}

export function* saga() {
    yield all([
        takeEvery(GET_USERS_LIST_REQUEST, getUsersListSaga),
        takeEvery(GET_USER_CARD_REQUEST, getUserCardSaga),
        takeEvery(CREATE_USER_REQUEST, createUserSaga),
        takeEvery(TOGGLE_USER_ACTIVE_REQUEST, toggleUserActiveSaga),
        takeEvery(GENERATE_KEY_OPEN_API_REQUEST, generateKeySaga),
    ]);
}
