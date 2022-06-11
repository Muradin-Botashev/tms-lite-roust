import React, { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import CardLayout from '../../components/CardLayout';
import { Button, Confirm, Icon, Modal, Popup } from 'semantic-ui-react';
import FormField from '../../components/BaseComponents';
import {
    canDeleteSelector,
    cardProgressSelector,
    cardSelector,
    clearDictionaryCard,
    columnsCardSelector,
    deleteDictionaryEntryRequest,
    errorSelector,
    getCardConfigRequest,
    getCardRequest,
    getDictionaryCardDefaultValueRequest,
    progressSelector,
    saveDictionaryCardRequest,
} from '../../ducks/dictionaryView';

const Content = ({ columns, error, form, handleChange }) => {
    return (
        <div className="ui form dictionary-edit">
            {columns.map(column => {
                let newColumn = {
                    ...column,
                };

                if (column.dependencies && column.dependencies.length) {
                    let sourceParams = {};

                    column.dependencies.forEach(item => {
                        sourceParams = {
                            ...sourceParams,
                            [item]:
                                form[item] && typeof form[item] === 'object'
                                    ? form[item].value
                                    : form[item],
                        };
                    });

                    newColumn = {
                        ...newColumn,
                        sourceParams,
                    };
                }
                return (
                    <FormField
                        {...newColumn}
                        noScrollColumn={column}
                        key={column.name}
                        error={error[column.name]}
                        value={form[column.name]}
                        onChange={handleChange}
                    />
                );
            })}
        </div>
    );
};

const CardNew = props => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const {
        match = {},
        history,
        location,
        load,
        isModal,
        openModal,
        onClose: onCloseModal,
    } = props;
    const { params = {} } = match;
    const { name, id } = params;
    const { state } = location;
    const { defaultForm, columns: propsColumns } = state;

    let [form, setForm] = useState({ ...defaultForm });
    let [confirmation, setConfirmation] = useState({ open: false });
    let [notChangeForm, setNotChangeForm] = useState(true);
    let [dependentFields, setDependentFields] = useState({});

    const columns = useSelector(state =>
        propsColumns ? propsColumns : columnsCardSelector(state, name, id),
    );
    const canDelete = useSelector(state => canDeleteSelector(state, name));
    const loading = useSelector(state => cardProgressSelector(state));
    const progress = useSelector(state => progressSelector(state));
    const card = useSelector(state => cardSelector(state));
    const error = useSelector(state => errorSelector(state));

    useEffect(() => {
        id && dispatch(getCardConfigRequest({ id, name }));
        id
            ? dispatch(getCardRequest({ id, name }))
            : dispatch(getDictionaryCardDefaultValueRequest(name));

        return () => {
            dispatch(clearDictionaryCard());
        };
    }, []);

    useEffect(() => {
        setForm(form => ({
            ...card,
            ...form,
        }));
    }, [card]);

    useEffect(() => {
        let obj = {};
        columns && columns.length && columns
            .filter(column => column.dependencies && column.dependencies.length)
            .forEach(column => {
                column.dependencies.forEach(item => {
                    obj = {
                        ...obj,
                        [item]: [...(obj[item] || []), column.name],
                    };
                });
            });

        setDependentFields(obj);
    }, [columns]);

    const onOpenModal = () => {};

    const title = useMemo(
        () => (id ? `${t(name)}: ${t('edit_record')}` : `${t(name)}: ${t('new_record')}`),
        [name, id],
    );

    const getActionsFooter = useCallback(() => {
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
    }, [form, notChangeForm, progress]);

    const handleSave = () => {
        let params = {
            ...form,
        };

        if (id) {
            params = {
                ...params,
                id,
            };
        }

        const callbackConfirmation = message => {
            setConfirmation({
                open: true,
                content: message,
                onCancel: confirmClose,
                onConfirm: () => {
                    confirmClose();
                    dispatch(
                        saveDictionaryCardRequest({
                            params,
                            name,
                            callbackSuccess: () => {
                                load && load(form);
                                onClose();
                            },
                            isConfirmed: true,
                        }),
                    );
                },
            });
        };

        dispatch(
            saveDictionaryCardRequest({
                params,
                name,
                callbackSuccess: () => {
                    load && load(form);
                    onClose();
                },
                isConfirmed: false,
                callbackConfirmation,
            }),
        );
    };

    const handleDelete = () => {
        /* const { id, deleteEntry, name } = this.props;*/

        dispatch(
            deleteDictionaryEntryRequest({
                name,
                id,
                callbackSuccess: onClose,
            }),
        );
    };

    const getActionsHeader = useCallback(() => {
        return (
            <div>
                {canDelete ? (
                    <Popup
                        content={t('delete')}
                        position="bottom right"
                        trigger={
                            <Button icon onClick={handleDelete}>
                                <Icon name="trash alternate outline" />
                            </Button>
                        }
                    />
                ) : null}
            </div>
        );
    }, []);

    const handleChange = useCallback(
        (event, { name, value, ...prev }) => {
            if (notChangeForm) {
                setNotChangeForm(false);
            }
            let formNew = {
                ...form,
                [name]: value,
            };

            if (dependentFields[name] && dependentFields[name].length) {
                dependentFields[name].forEach(item => {
                    formNew = {
                        ...formNew,
                        [item]: "",
                    }
                })
            }

            setForm(formNew);
        },
        [notChangeForm, form, dependentFields],
    );

    const confirmClose = () => {
        setConfirmation({ open: false });
    };

    const onClose = () => {
        isModal
            ? onCloseModal()
            : history.replace({
                  pathname: location.state.pathname,
                  state: {
                      ...location.state,
                      pathname: location.state.gridLocation
                          ? location.state.gridLocation
                          : location.state.pathname,
                  },
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

    return (
        <>
            {isModal ? (
                <Modal
                    dimmer="blurring"
                    open={openModal}
                    closeOnDimmerClick={false}
                    onOpen={onOpenModal}
                    onClose={onCloseModal}
                    closeIcon
                >
                    <Modal.Header>{title}</Modal.Header>
                    <Modal.Description>
                        {/*<Loader size="huge" active={loading}>
                            Loading
                        </Loader>*/}
                        <Content
                            columns={columns}
                            error={error}
                            form={form}
                            handleChange={handleChange}
                        />
                    </Modal.Description>
                    <Modal.Actions>{getActionsFooter()}</Modal.Actions>
                </Modal>
            ) : (
                <CardLayout
                    title={title}
                    actionsFooter={getActionsFooter}
                    actionsHeader={getActionsHeader}
                    onClose={handleClose}
                    loading={loading}
                >
                    <Content
                        columns={columns}
                        error={error}
                        form={form}
                        handleChange={handleChange}
                    />
                </CardLayout>
            )}
            <Confirm
                dimmer="blurring"
                open={confirmation.open}
                onCancel={confirmation.onCancel}
                cancelButton={t('cancelConfirm')}
                confirmButton={t('Yes')}
                onConfirm={confirmation.onConfirm}
                content={confirmation.content}
            />
        </>
    );
};

export default CardNew;
