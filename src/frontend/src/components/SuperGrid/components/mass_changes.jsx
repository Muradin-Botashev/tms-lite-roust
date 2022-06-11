import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useDispatch, useSelector } from 'react-redux';
import { Button, Dropdown, Form, Grid, Icon } from 'semantic-ui-react';
import {
    invokeMassUpdateRequest,
    progressMassUpdateSelector,
    updatesSelector,
} from '../../../ducks/gridActions';
import FormField from '../../BaseComponents';
import { TEXT_TYPE } from '../../../constants/columnTypes';

const MassChanges = ({ gridName, load }) => {
    const { t } = useTranslation();
    const dispatch = useDispatch();
    const updates = useSelector(state => updatesSelector(state)) || [];
    const progress = useSelector(state => progressMassUpdateSelector(state)) || false;
    let [field, setField] = useState(null);
    let [column, setColumn] = useState({});
    let [changValue, setValue] = useState(null);
    let [isDisabledBtn, setIsDisabledBtn] = useState(true);

    const fieldParams = updates.find(item => item.name === field);

    useEffect(
        () => {
            setValue(null);
            setColumn({
                name: field,
                type: fieldParams ? fieldParams.type : TEXT_TYPE,
                source: fieldParams ? fieldParams.source : '',
                noLabel: true,
                isDisabled: !field,
                placeholder: t('new_value'),
            });
        },
        [field],
    );

    useEffect(() => {
        isDisabledBtn && setIsDisabledBtn(false);
    }, [field, changValue]);

    const handleSave = () => {
        dispatch(
            invokeMassUpdateRequest({
                name: gridName,
                field,
                ids: fieldParams.ids,
                value: changValue,
                callbackSuccess: load,
            }),
        );
        setIsDisabledBtn(true);
    };

    return (
        <Form className="grid-mass-updates">
            {updates.length ? (
                <Grid>
                    <Grid.Row columns="equal">
                        <Grid.Column className="grid-mass-updates-fields">
                            <label>{t('change')}</label>
                            <Dropdown
                                placeholder={t('choose_option')}
                                fluid
                                selection
                                value={field}
                                upward
                                options={updates.map(item => ({
                                    key: item.name,
                                    value: item.name,
                                    text: t(item.name),
                                }))}
                                onChange={(e, { value }) => setField(value)}
                            />
                        </Grid.Column>
                        <Grid.Column className="grid-mass-updates-fields">
                            <FormField
                                {...column}
                                value={changValue}
                                upward
                                isBulkUpdateOnly
                                onChange={(e, { name, value }) => setValue(value)}
                            />
                            <Button
                                icon
                                disabled={changValue === null || changValue === undefined || isDisabledBtn}
                                className="grid-mass-updates-save"
                                loading={progress}
                                onClick={handleSave}
                            >
                                <Icon name="save" />
                            </Button>
                        </Grid.Column>
                    </Grid.Row>
                </Grid>
            ) : null}
        </Form>
    );
};

export default MassChanges;
