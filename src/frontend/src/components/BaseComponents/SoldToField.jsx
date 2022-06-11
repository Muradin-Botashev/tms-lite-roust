import React, { useEffect, useState } from 'react';
import Select from './Select';
import { useDispatch, useSelector } from 'react-redux';
import { getLookupRequest, valuesListSelector } from '../../ducks/lookup';
import { columnsSelector } from '../../ducks/dictionaryView';
import { userPermissionsSelector } from '../../ducks/profile';
import Card from '../../containers/customDictionary/card';
import { Button, Icon, Popup } from 'semantic-ui-react';
import { SETTINGS_TYPE_EDIT } from '../../constants/formTypes';
import { useTranslation } from 'react-i18next';
import { addError, clearError } from '../../ducks/gridCard';

const SoldToField = props => {
    const { value, settings, error, textValue, deliveryAddress, onChange, name, goToSoldTo } = props;
    const dispatch = useDispatch();
    const { t } = useTranslation();

    let [modalOpen, setModalOpen] = useState(false);
    let [notSoldTo, setNotSoldTo] = useState(false);

    const valuesList = useSelector(state => valuesListSelector(state, 'soldTo'));
    const soldToItem = value ? valuesList.find(item => item.value === value.value) || {} : {};
    const columns = useSelector(state => columnsSelector(state, 'warehouses')) || [];
    const columnsEdit = columns.map(item => {
        if (item.name === 'soldToNumber') {
            return {
                ...item,
                isDisabled: true,
            };
        } else return item;
    });
    const defaultForm = {
        soldToNumber: textValue ? textValue.name : null,
        address: deliveryAddress,
    };
    const userPermissions = useSelector(state => userPermissionsSelector(state));

   /* const handleLoad = form => {
        dispatch(
            getLookupRequest({
                name: 'soldTo',
                isForm: true,
                callbackSuccess: result => {
                    const newSoldTo = result.find(item => item.value === form.soldToNumber);

                    onChange(null, {
                        value: {
                            ...newSoldTo,
                            value: form.soldToNumber,
                            name: form.soldToNumber,
                        },
                        name,
                    });
                },
            }),
        );
    };*/

    useEffect(() => {
        dispatch(
            getLookupRequest({
                name: 'soldTo',
                isForm: true,
            }),
        );
    }, []);

    useEffect(
        () => {
            if (
                value &&
                valuesList.length &&
                !valuesList.find(item => item.value === value.value) &&
                !error
            ) {
                dispatch(
                    addError({
                        name: 'soldTo',
                        message: t('soldTo_error'),
                    }),
                );
                setNotSoldTo(true);
            } else if (
                error &&
                value &&
                valuesList.length &&
                valuesList.find(item => item.value === value.value)
            ) {
                dispatch(clearError('soldTo'));
                setNotSoldTo(false);
            }
        },
        [valuesList, value],
    );

    const handleOpenModal = () => {
        //setModalOpen(true);
        goToSoldTo(soldToItem.id, {
            columns: notSoldTo ? columns : columnsEdit,
            defaultForm: notSoldTo ? defaultForm : null
        });
    };

    const handleCloseModal = () => {
        setModalOpen(false);
    };

    return (
        <Select {...props}>
            {userPermissions.includes(15) && settings === SETTINGS_TYPE_EDIT ? (
                notSoldTo ? (
                    <Popup
                        content={t('Add to the catalog Delivery warehouses')}
                        position="bottom right"
                        trigger={
                            <div>
                                <Button icon onClick={handleOpenModal}>
                                    <Icon name="add" />
                                </Button>
                            </div>
                        }
                    />
                ) : (
                    <Popup
                        content={t('Edit delivery warehouse data')}
                        position="bottom right"
                        trigger={
                            <div>
                                <Button icon onClick={handleOpenModal}>
                                    <Icon name="edit" />
                                </Button>
                            </div>
                        }
                    />
                )
            ) : null}
            {/* <Card
                title={`${t('warehouses')}: ${t('edit_record')}`}
                columns={columnsEdit}
                name="warehouses"
                id={soldToItem.id}
                load={handleLoad}
            />
            <Card
                title={`${t('warehouses')}: ${t('new_record')}`}
                columns={columns}
                name="warehouses"
                defaultForm={defaultForm}
                load={handleLoad}
            >*/}
            {/*{modalOpen && (
                <Card
                    openModal={modalOpen}
                    isModal
                    match={{
                        params: {
                            name: 'warehouses',
                            id: notSoldTo ? null : soldToItem.id,
                        },
                    }}
                    load={handleLoad}
                    columns={notSoldTo ? columns : columnsEdit}
                    defaultForm={notSoldTo ? defaultForm : null}
                    onClose={handleCloseModal}
                />
            )}*/}
        </Select>
    );
};

export default SoldToField;
