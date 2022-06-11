import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { Confirm, Dimmer, Form, Loader, Grid, Button } from 'semantic-ui-react';
import FormField from '../../components/BaseComponents';
import CardLayout from '../../components/CardLayout';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { SELECT_TYPE, TEXT_TYPE } from '../../constants/columnTypes';
import {
    clearUserCard,
    getUserCardRequest,
    progressSelector,
    userCardSelector,
    errorSelector,
    createUserRequest,
    saveProgressSelector, generateKeyRequest,
} from '../../ducks/users';
import PasswordField from "../../components/BaseComponents/Password";
import {toast} from "react-toastify";

const UserCard = props => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const { match, history, location } = props;
    const { params = {} } = match;
    const { id } = params;

    let [form, setForm] = useState({});
    let [confirmation, setConfirmation] = useState({ open: false });
    let [notChangeForm, setNotChangeForm] = useState(true);

    const loading = useSelector(state => progressSelector(state));
    const progress = useSelector(state => saveProgressSelector(state));
    const user = useSelector(state => userCardSelector(state));
    const error = useSelector(state => errorSelector(state)) || {};

    useEffect(() => {
        id && dispatch(getUserCardRequest(id));

        return () => {
            dispatch(clearUserCard());
        };
    }, []);

    useEffect(
        () => {
            setForm(form => ({
                ...form,
                ...user,
            }));
        },
        [user],
    );

    const title = useMemo(
        () => (id ? t('edit_user', { name: user.userName }) : `${t('create_user_title')}`),
        [id, user],
    );

    const getActionsFooter = useCallback(
        () => {
            return (
                <>
                    <Button color="grey" onClick={handleClose}>
                        {t('CancelButton')}
                    </Button>
                    <Button
                        color="blue"
                        disabled={notChangeForm}
                        loading={progress}
                        onClick={handleSave}
                    >
                        {t('SaveButton')}
                    </Button>
                </>
            );
        },
        [form, notChangeForm, progress],
    );

    const handleSave = () => {
        let params = { ...form };

        if (id) {
            params = {
                ...params,
                id,
            };
        }

        dispatch(
            createUserRequest({
                params,
                callbackFunc: onClose,
            }),
        );
    };

    const handleChange = useCallback(
        (event, {name, value}) => {
            if (notChangeForm) {
                setNotChangeForm(false);
            }
            setForm(form => ({
                ...form,
                [name]: value,
            }));
        },
        [notChangeForm, form],
    );

    const handleRoleChange = useCallback((event, { name, value }) => {
        handleChange(event, { name, value });

        if (name === 'companyId') {
            handleChange(event, { name: 'carrierId', value: null });
            handleChange(event, { name: 'roleId', value: null });
        }

    }, []);

    const confirmClose = () => {
        setConfirmation({ open: false });
    };

    const onClose = () => {
        history.push({
            pathname: location.state.pathname,
            state: { ...location.state },
        });
    };

    const handleClose = () => {
        if (notChangeForm) {
            onClose();
        } else {
            setConfirmation({
                open: true,
                content: t('confirm_close_dictionary'),
                onCancel: confirmClose,
                onConfirm: onClose,
            });
        }
    };

    const generateKey = () => {
        dispatch(generateKeyRequest({
            id,
            callbackSuccess: token => {
                navigator.clipboard &&
                navigator.clipboard.writeText(token).then(
                    () => {
                        toast.info(t('copied_to_clipboard_success'));
                    },
                    error => {
                        toast.error(t('copied_to_clipboard_error', { error }));
                    },
                );
            }
        }))
    };

    return (
        <CardLayout
            title={title}
            actionsFooter={getActionsFooter}
            onClose={handleClose}
            loading={loading}
        >
            <Form className="user-form">
                <FormField
                    type={TEXT_TYPE}
                    name="login"
                    value={form['login']}
                    isRequired
                    error={error['login']}
                    onChange={handleChange}
                />
                <div className="form-group-btn">
                    <PasswordField
                        typeValue="password"
                        name="password"
                        text={!user.id ? "password" : "set_new_password"}
                        isRequired={!user.id}
                        value={form['password']}
                        type={TEXT_TYPE}
                        error={error['password']}
                        autoComplete="new-password"
                        onChange={handleChange}
                    />
                    <Button onClick={generateKey}>{t('generate_key_open_api')}</Button>
                </div>
                <FormField
                    type={TEXT_TYPE}
                    name="email"
                    value={form['email']}
                    isRequired
                    error={error['email']}
                    onChange={handleChange}
                />
                <FormField
                    name="userName"
                    value={form['userName']}
                    type={TEXT_TYPE}
                    isRequired
                    error={error['userName']}
                    onChange={handleChange}
                />
                <FormField
                    fluid
                    search
                    selection
                    name="companyId"
                    value={form['companyId']}
                    source="companies"
                    error={error['companyId']}
                    type={SELECT_TYPE}
                    onChange={handleChange}
                />
                <FormField
                    fluid
                    search
                    selection
                    text="role"
                    name="roleId"
                    value={form['roleId']}
                    source="roles"
                    sourceParams={{companyId: form['companyId'] && form['companyId'].value}}
                    isRequired
                    error={error['roleId']}
                    type={SELECT_TYPE}
                    onChange={handleRoleChange}
                />
                <FormField
                    fluid
                    search
                    selection
                    upward
                    name="carrierId"
                    value={form['carrierId']}
                    source="transportCompanies"
                    sourceParams={{companyId: form['companyId'] && form['companyId'].value}}
                    error={error['carrierId']}
                    type={SELECT_TYPE}
                    onChange={handleChange}
                />
                {/*{id ? (
                                            <Label pointing>
                                                Оставьте поле пустым, если не хотите менять пароль
                                            </Label>
                                        ) : null}*/}
                <Form.Field>
                    <Form.Checkbox
                        label={t('isActive')}
                        name="isActive"
                        checked={form['isActive']}
                        onChange={(e, { name, checked }) =>
                            handleChange(e, { name, value: checked })
                        }
                    />
                </Form.Field>
            </Form>
            <Confirm
                dimmer="blurring"
                open={confirmation.open}
                onCancel={confirmation.onCancel}
                cancelButton={t('cancelConfirm')}
                confirmButton={t('Yes')}
                onConfirm={confirmation.onConfirm}
                content={confirmation.content}
            />
        </CardLayout>
    );
};

export default UserCard;
